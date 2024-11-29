@echo off
setlocal enabledelayedexpansion

set "paths="
for %%i in (%*) do (
    set "paths=!paths!%%~i"
    set "paths=!paths! "
)

rem Remove the trailing space
set "paths=%paths:~0,-1%"

echo|set /p="%paths%"|clip
