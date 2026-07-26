@echo off
title SafeScribe AI (.NET 10 Avalonia UI Cross-Platform Desktop App)
cd /d "%~dp0"
echo Launching SafeScribe AI (.NET 10 Avalonia UI Desktop App)...
dotnet run --project SafeScribe.UI.Avalonia/SafeScribe.UI.Avalonia.csproj
pause
