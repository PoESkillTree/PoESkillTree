@ECHO OFF
@REM Cleans up everything dist-release script created.

WHERE /Q dotnet
IF ERRORLEVEL% == 1 (
	ECHO ERROR: Command dotnet not found.
	GOTO :END
)

dotnet msbuild -verbosity:normal release.xml /target:Clean

:END
