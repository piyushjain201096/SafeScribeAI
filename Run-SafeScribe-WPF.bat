@echo off
title SafeScribe AI (.NET WPF Desktop App)
cd /d "%~dp0"
echo Launching SafeScribe AI (.NET 10 WPF Desktop App)...
dotnet run --project SafeScribe.UI/SafeScribe.UI.csproj
pause
