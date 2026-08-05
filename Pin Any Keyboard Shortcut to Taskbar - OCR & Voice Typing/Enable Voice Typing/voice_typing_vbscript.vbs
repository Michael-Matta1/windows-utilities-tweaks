' VBScript to silently run PowerShell Voice Typing script
' This script runs the PowerShell script completely hidden

Dim objShell, strCommand, strPowerShellPath

' Create Shell object
Set objShell = CreateObject("WScript.Shell")

' Get the directory where this VBS script is located
strScriptPath = Left(WScript.ScriptFullName, InStrRev(WScript.ScriptFullName, "\"))

' PowerShell script filename (assumes it's in the same directory)
strPowerShellScript = strScriptPath & "VoiceTyping.ps1"

' Build the command to run PowerShell silently
strCommand = "powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File """ & strPowerShellScript & """"

' Run the command completely hidden (0 = hidden window, False = don't wait)
objShell.Run strCommand, 0, False

' Clean up
Set objShell = Nothing