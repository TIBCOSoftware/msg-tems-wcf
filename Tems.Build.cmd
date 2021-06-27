@echo off
rem
rem Command file for compiling the Tems.sln solution.
rem
rem Usage: Tems.Build.cmd [config]
rem        If the build config name is provided (i.e. Debug or Release) then the
rem        build is run without any user prompts and exits when complete.
rem        If config is not specified, a prompt is displayed for the config
rem        name and a user prompt is displayed before exiting when complete.
rem

:Start
echo ********************************************************************************
echo                      Tems Solution Compile and Build Utility
echo.
echo ********************************************************************************
echo.
echo.
echo If running the compile with a single parallel project build is required use
echo the following steps to set this option:
echo.
echo     - Click on build.sln
echo       - Tools : Options...
echo         - Project and Solutions
echo           - Build and Run
echo             - [ set to 1 ] maximum number of parallel project build
echo.

:Set VS path
rem Set the mvs_dir to the correct path on the build machine.

rem set mvs_dir=D:\VisualStudio2008\Common7
rem set mvs_dir=C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7
set mvs_dir=C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7


echo mvs_dir is set to: %mvs_dir%
echo.

rem Skip user input if config is specified as command line argument.
if not "%1"=="" (
    set config=%1
    goto Set MS environment variables      
)

:SetConfig
set config=Release
set /p config=Set build configuration: [R]elease, [D]ebug, default=R 

if /i %config%==R (
    set config=Release
) else (
    if /i %config%==D (
        set config=Debug
    )
)
echo.
echo.
echo Build configuration set to: %config%
echo.
echo.

set continue=yes
set /p continue=Press any key to continue or X to exit: 
if /i %continue%==X (
    goto Exit
)

echo.
echo.

:Set MS environment variables
rem call "%mvs_dir%\Tools\vsvars32.bat"
call "%mvs_dir%\Tools\VsDevCmd.bat"

:Run Build
echo.
echo Running build command:
echo.
echo     "%mvs_dir%\IDE\devenv" Tems.sln /Rebuild %config%
echo.
echo.

"%mvs_dir%\IDE\devenv" Tems.sln /Rebuild %config%

echo.
echo.

:Copy Artifacts
copy .\NetTemsActivator\bin\%config%\NetTemsActivator.exe BuildArtifacts\
copy .\TemsTransport\bin\%config%\TIBCO.EMS.WCF.dll BuildArtifacts\

:Build complete
echo Build complete.
echo.
echo The BuildArtifacts directory should contain:
echo     NetTemsActivator.exe
echo     TIBCO.EMS.WCF.dll
echo.
echo.

dir BuildArtifacts

echo.

rem Skip user prompt if config is specified as command line argument.
if "%1"=="" (
    set /p continue=Press any key to exit.       
)

:Exit
