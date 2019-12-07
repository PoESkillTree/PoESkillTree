@ECHO OFF
@REM Updates bin/Debug/netcoreapp3.0/Data/Equipment/Assets/ using UpdateDB tool.

dotnet run -c release -p ..\UpdateDB /ItemImages /SpecifiedDir:bin/Debug/netcoreapp3.0

:END
