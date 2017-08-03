@ECHO OFF
@REM Updates bin/Debug/Data/Equipment/Assets/ using UpdateDB tool.

@REM Check whether Git is found in PATH
WHERE /Q git
IF ERRORLEVEL% == 1 (
	ECHO ERROR: Command git not found.
	ECHO Please add Git for Windows binaries to PATH environment variable.
	GOTO :END
)

@REM Check whether vsdevcmd is found in PATH
WHERE /Q vsdevcmd
IF ERRORLEVEL% == 1 (
	ECHO ERROR: Command vsdevcmd not found.
	ECHO Please add Visual Studio's Common7\Tools directory to PATH environment variable.
	GOTO :END
)

@REM Run in separate process
cmd /C "vsdevcmd.bat && cd ""%~dp0"" && msbuild release.xml /target:UpdateItemImages"

:END
