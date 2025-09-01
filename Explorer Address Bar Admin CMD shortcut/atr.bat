@echo off
powershell -Command "Start-Process wt -ArgumentList '-d','\"%CD%\"' -Verb RunAs"