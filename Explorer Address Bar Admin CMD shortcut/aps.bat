@echo off
REM source of this file is https://github.com/Michael-Matta1/windows-utilities-tweaks (c) Michael-Matta1
powershell -Command "Start-Process powershell -ArgumentList '-NoExit','-Command','Set-Location ''%CD%''' -Verb RunAs"