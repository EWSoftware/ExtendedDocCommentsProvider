@ECHO OFF

SETLOCAL

CD Source

REM Use MSBuild from whatever edition of Visual Studio is installed
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\bin\MSBuild.exe"

IF EXIST "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current" SET "MSBUILD2022=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current" SET "MSBUILD2022=%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current" SET "MSBUILD2022=%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\bin\MSBuild.exe"

IF NOT EXIST "%MSBUILD%" GOTO Build2022

ECHO *
ECHO * VS2019 package
ECHO *

"%MSBUILD%" ExtendedDocCommentsProvider2019.sln /nologo /v:m /m /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

:Build2022
IF NOT EXIST "%MSBUILD2022%" GOTO BuildDocs

ECHO *
ECHO * VS2022 and later package
ECHO *

"%MSBUILD2022%" ExtendedDocCommentsProvider2022.sln /nologo /v:m /m /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

:BuildDocs
CD ..\

IF NOT EXIST "%MSBUILD%" SET "MSBUILD=%MSBUILD2022%"

IF NOT "%SHFBROOT%"=="" "%MSBUILD%" /nologo /v:m "Docs\ExtendedDocCommentsProviderDocs.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

IF "%SHFBROOT%"=="" ECHO **** Sandcastle help file builder not installed.  Skipping help build. ****

:End

ENDLOCAL
