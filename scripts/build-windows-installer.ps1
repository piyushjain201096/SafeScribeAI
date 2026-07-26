<#
.SYNOPSIS
    Builds the Windows single-file executable and installer package for SafeScribe AI.
#>

[CmdletBinding()]
param(
    [string]$Version = "1.0.0",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path "$PSScriptRoot\.."
Set-Location $RepoRoot

Write-Host "=========================================================" -ForegroundColor Cyan
Write-Host " Building SafeScribe AI Windows Package (v$Version)      " -ForegroundColor Cyan
Write-Host "=========================================================" -ForegroundColor Cyan

$PublishDir = Join-Path $RepoRoot "publish\win-x64"
$DistDir    = Join-Path $RepoRoot "dist"

if (Test-Path $PublishDir) { 
    Remove-Item -Path $PublishDir -Recurse -Force -ErrorAction SilentlyContinue 
}
if (-not (Test-Path $DistDir)) { 
    New-Item -Path $DistDir -ItemType Directory -Force | Out-Null 
}

Write-Host "`n[1/3] Publishing .NET 10 Avalonia App (win-x64)..." -ForegroundColor Yellow
dotnet publish "$RepoRoot\SafeScribe.UI.Avalonia\SafeScribe.UI.Avalonia.csproj" `
    -c $Configuration `
    -r win-x64 `
    --self-contained `
    -p:PublishSingleFile=true `
    -p:Version=$Version `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed!"
    exit 1
}

Write-Host "`n[2/3] Creating Portable Zip Archive..." -ForegroundColor Yellow
$ZipPath = Join-Path $DistDir "SafeScribeAI-Portable-v$Version-win-x64.zip"
if (Test-Path $ZipPath) { Remove-Item -Path $ZipPath -Force }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($PublishDir, $ZipPath, [System.IO.Compression.CompressionLevel]::Optimal, $false)
Write-Host "[OK] Portable zip created at: $ZipPath" -ForegroundColor Green

Write-Host "`n[3/3] Building Inno Setup Installer (.exe)..." -ForegroundColor Yellow
$IsccExe = $null

$cmd = Get-Command "iscc" -ErrorAction SilentlyContinue
if ($cmd) {
    $IsccExe = $cmd.Source
} elseif (Test-Path "C:\Program Files (x86)\Inno Setup 6\ISCC.exe") {
    $IsccExe = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
} elseif (Test-Path "C:\Program Files\Inno Setup 6\ISCC.exe") {
    $IsccExe = "C:\Program Files\Inno Setup 6\ISCC.exe"
}

if ($IsccExe) {
    Write-Host "Using Inno Setup Compiler: $IsccExe" -ForegroundColor Cyan
    $IssScript = Join-Path $PSScriptRoot "SafeScribe-InnoSetup.iss"
    & $IsccExe "/DMyAppVersion=$Version" $IssScript
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Windows Installer created successfully in 'dist/'!" -ForegroundColor Green
    } else {
        Write-Warning "Inno Setup compiler encountered an error."
    }
} else {
    Write-Host "Inno Setup (ISCC.exe) not detected. (Install Inno Setup 6 to compile .exe installer)" -ForegroundColor Yellow
    Write-Host "[OK] Portable zip package is ready in 'dist/'" -ForegroundColor Green
}

Write-Host "`n=========================================================" -ForegroundColor Cyan
Write-Host " Build Output Summary:" -ForegroundColor Cyan
Get-ChildItem -Path $DistDir | ForEach-Object {
    $sizeMB = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  - $($_.Name) ($sizeMB MB)" -ForegroundColor White
}
Write-Host "=========================================================" -ForegroundColor Cyan
