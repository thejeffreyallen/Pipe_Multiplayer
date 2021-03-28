@echo off
:Start
"PIPE_Valve_Online_Server.exe" 50 7777 60
:: Wait 5 seconds before restarting.
TIMEOUT /T 5
GOTO:Start