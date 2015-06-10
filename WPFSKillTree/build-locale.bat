@ECHO OFF
@REM Creates template Messages file and updates existing translation catalogs.
@REM When invoked with argument, it will also copy Locale folder to target directory specified by argument.

PUSHD %~dp0

IF [%1] == [] cmd /C "vsvars32.bat && msbuild release.xml /target:BuildLocale"
IF NOT [%1] == [] cmd /C "vsvars32.bat && msbuild release.xml /target:BuildAndCopyLocale /property:LocaleTargetDir=%1"

POPD
