@echo off
:Start 
Set /A players=30
Set /A port=7780
Set /A tick=120

"../PIPE_Valve_Online_Server.exe" %players% %port% %tick% 


GOTO:Start