@echo off
REM source of this file is https://github.com/Michael-Matta1/windows-utilities-tweaks (c) Michael-Matta1
powershell -Command "Start-Process cmd -ArgumentList '/k','cd /d %CD%' -Verb RunAs"
