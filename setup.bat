@echo off

:: Build GameEngines.sln solution so that the dlls could be generated and copied to the Unity project
dotnet build GameEngines.sln --configuration Debug --verbosity q

pause
exit 0