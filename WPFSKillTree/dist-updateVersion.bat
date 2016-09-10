@ECHO OFF
@REM Updates the version information and related strings in AssemblyInfo based on the data in Version.resx
@REM This normally only happens on release but may be required if data file structures are changed to enable conversion.

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
cmd /C "vsvars32.bat && msbuild release.xml /target:UpdateAssemblyInfo"
cmd /C "vsvars32.bat && msbuild release.xml /target:Clean"

:END
