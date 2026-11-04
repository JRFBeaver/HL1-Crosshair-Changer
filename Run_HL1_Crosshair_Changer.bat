@echo off
cd /d "%~dp0"
title Half-Life Crosshair Changer v4.0 Builder

set "EXE_NAME=HL1_Crosshair_Changer.exe"

set "CSC_PATH="
if exist "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" set "CSC_PATH=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if "%CSC_PATH%"=="" if exist "%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe" set "CSC_PATH=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if "%CSC_PATH%"=="" (
    echo [ERROR] Microsoft .NET Framework v4.0 compiler not found.
    pause
    exit /b 1
)

if not exist "%~dp0HL1_Crosshair_Changer_src.cs" (
    echo [ERROR] HL1_Crosshair_Changer_src.cs not found! Please extract the entire ZIP archive first.
    pause
    exit /b 1
)

del /f /q "%~dp0%EXE_NAME%" >nul 2>&1

set "ICON_ARG="
if exist "%~dp0HL1_Crosshair_Icon.ico" set "ICON_ARG=/win32icon:"%~dp0HL1_Crosshair_Icon.ico""

"%CSC_PATH%" /nologo /target:winexe %ICON_ARG% /out:"%~dp0%EXE_NAME%" /r:System.Windows.Forms.dll /r:System.Drawing.dll "%~dp0HL1_Crosshair_Changer_src.cs"

if exist "%~dp0%EXE_NAME%" (
    start "" "%~dp0%EXE_NAME%"
) else (
    echo [ERROR] Failed to build %EXE_NAME%. Make sure the program is closed and try running as Administrator.
    pause
)
