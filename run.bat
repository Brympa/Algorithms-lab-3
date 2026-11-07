@echo off
chcp 65001 >nul
setlocal

where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] .NET SDK NOT FOUND IN PATH!
    echo Download .NET SDK 9 from https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)
echo [1/2] Build test...
dotnet build -c Release --nologo
if %ERRORLEVEL% neq 0 (
    echo [ERROR] The build failed with an error!
    pause
    exit /b 1
)
echo [2/2] Start...
dotnet run -c Release
pause