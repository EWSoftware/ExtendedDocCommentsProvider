@ECHO OFF

SETLOCAL

CD Source

ECHO *
ECHO * VS2019 and later package
ECHO *

REM Use MSBuild from whatever edition of Visual Studio is installed
IF EXIST "%ProgramFiles%\Microsoft Visual Studio\2022\Preview\MSBuild\Current" SET "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Preview\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\bin\MSBuild.exe"

IF NOT EXIST "%MSBUILD%" GOTO End

"%MSBUILD%" ExtendedDocCommentsProvider.sln /nologo /v:m /m /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

CD ..\

IF NOT "%SHFBROOT%"=="" "%MSBUILD%" /nologo /v:m "Docs\ExtendedDocCommentsProviderDocs.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

IF "%SHFBROOT%"=="" ECHO **** Sandcastle help file builder not installed.  Skipping help build. ****

:End

ENDLOCAL
