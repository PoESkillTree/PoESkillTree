@ECHO OFF
@REM Cleans up everything dist-release script created.

@REM Check whether vsdevcmd is found in PATH
WHERE /Q vsdevcmd
IF ERRORLEVEL% == 1 (
	ECHO ERROR: Command vsdevcmd not found.
	ECHO Please add Visual Studio's Common7\Tools directory to PATH environment variable.
	GOTO :END
)

@REM Run in separate process
cmd /C "vsdevcmd.bat && cd ""%~dp0"" && msbuild release.xml /target:Clean"

:END
