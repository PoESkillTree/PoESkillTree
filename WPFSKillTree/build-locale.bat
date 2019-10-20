@ECHO OFF
@REM Creates template Messages file and updates existing translation catalogs.

dotnet msbuild -verbosity:normal release.xml /target:BuildLocale
