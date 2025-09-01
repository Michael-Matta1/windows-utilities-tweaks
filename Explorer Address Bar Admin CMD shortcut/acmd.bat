@echo off
powershell -Command "Start-Process cmd -ArgumentList '/k','cd /d %CD%' -Verb RunAs"
