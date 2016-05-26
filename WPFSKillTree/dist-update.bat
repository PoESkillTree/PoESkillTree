@ECHO OFF
@REM Updates Data/Equipment/GemList.xml file using UpdateDB tool.

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
cmd /C "vsvars32.bat && msbuild release.xml /target:Update"

:END
