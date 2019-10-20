@ECHO OFF
@REM Creates new release package.
@REM 1) Change version string in WPFSKillTree.csproj to reflect new release version.
@REM 2) Run this script.
@REM 3) See dist\<release-package>.zip

WHERE /Q git
IF ERRORLEVEL% == 1 (
	ECHO ERROR: Command git not found.
	ECHO Please add Git for Windows binaries to PATH environment variable.
	GOTO :END
)

WHERE /Q dotnet
IF ERRORLEVEL% == 1 (
	ECHO ERROR: Command dotnet not found.
	GOTO :END
)

dotnet msbuild -verbosity:normal release.xml /target:Release

:END
