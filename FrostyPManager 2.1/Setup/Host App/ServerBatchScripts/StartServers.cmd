@echo off
SET "Str=C:\Program Files (x86)\Steam\steamapps\PIPE\Host App\ServerBatchScripts"
FOR %%i IN (*.bat) DO start cmd /k Call "%%i"