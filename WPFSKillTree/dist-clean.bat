@ECHO OFF
@REM Cleans up everything dist-release script created.

@REM Check whether vsvars32 is found in PATH
WHERE /Q vsvars32
IF ERRORLEVEL% == 1 (
	ECHO ERROR: Command vsdevcmd not found.
	ECHO Please add Visual Studio's Common7\Tools directory to PATH environment variable.
	GOTO :END
)

@REM Run in separate process
cmd /C "vsvars32.bat && msbuild release.xml /target:Clean"

:END
