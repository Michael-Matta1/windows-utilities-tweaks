@echo off
REM source of this file is https://github.com/Michael-Matta1/windows-utilities-tweaks (c) Michael-Matta1
setlocal enabledelayedexpansion

set "paths="
for %%i in (%*) do (
    set "paths=!paths!%%~i"
    set "paths=!paths! "
)

rem Remove the trailing space
set "paths=%paths:~0,-1%"

echo|set /p="%paths%"|clip
