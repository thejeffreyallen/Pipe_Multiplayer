@echo off
:Start 
Set /A players=15
Set /A port=7779
Set /A tick=120

"../PIPE_Valve_Online_Server.exe" %players% %port% %tick% 


GOTO:Start