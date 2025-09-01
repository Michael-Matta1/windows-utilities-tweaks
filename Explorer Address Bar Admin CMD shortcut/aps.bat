@echo off
powershell -Command "Start-Process powershell -ArgumentList '-NoExit','-Command','Set-Location ''%CD%''' -Verb RunAs"