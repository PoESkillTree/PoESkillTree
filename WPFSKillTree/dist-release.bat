@ECHO OFF
@REM Creates new release package.
@REM 1) Change version string in Properties\Version.resx to reflect new release version.
@REM 2) Run this script.
@REM 3) See dist\<release-package>.zip

@REM Check whether Git is found in PATH
WHERE /Q git
IF ERRORLEVEL% == 1 (
	ECHO ERROR: Command git not found.
	ECHO Please add Git for Windows binaries to PATH environment variable.
	GOTO :END
)

@REM Check whether vsvars32 is found in PATH
WHERE /Q vsvars32
IF ERRORLEVEL% == 1 (
	ECHO ERROR: Command vsdevcmd not found.
	ECHO Please add Visual Studio's Common7\Tools directory to PATH environment variable.
	GOTO :END
)

@REM Run in separate process
cmd /C "vsvars32.bat && msbuild release.xml /target:Release"

:END
