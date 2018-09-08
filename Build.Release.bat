@echo off

C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe AlarmWorkflow-TestOperation.sln /p:Configuration=Release /verbosity:minimal /p:DebugSymbols=false /p:DebugType=None

pause