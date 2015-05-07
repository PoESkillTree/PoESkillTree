@ECHO OFF
@REM Creates template Messages file and updates existing translation catalogs.

cmd /C "vsvars32.bat && msbuild release.xml /target:BuildLocale"
