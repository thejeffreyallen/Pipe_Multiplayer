@echo off
PIPE_Valve_Online_Server.exe 50 7777 120 xxx Pipeserver y n Post Put apikey

:: Wait 5 seconds before restarting.
TIMEOUT /T 5
GOTO:Start