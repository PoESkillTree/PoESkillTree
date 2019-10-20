@ECHO OFF
@REM Creates template Messages file and updates existing translation catalogs.

PUSHD %~dp0

cmd /C "vsdevcmd.bat && cd ""%~dp0"" && msbuild release.xml /target:BuildLocale"

POPD
