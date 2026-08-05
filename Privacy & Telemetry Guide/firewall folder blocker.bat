@echo off
setlocal enableextensions
if "%~1"=="" (set TARGET=%~dp0) else (set TARGET=%~1)
cd /d "%TARGET%"

for /R %%f in (*.exe) do (
  netsh advfirewall firewall add rule name="Blocked: %%f" dir=out program="%%f" action=block
  netsh advfirewall firewall add rule name="Blocked: %%f" dir=in program="%%f" action=block
)