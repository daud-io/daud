rd /s /q game
flatc --js game.fbs
flatc --csharp --gen-onefile game.fbs
copy game_generated.js ..\Game.Engine\wwwroot\js


REM if your flatc generator is generating namespace issues, you need to use the flatc.exe in 
REM the KdSoft.FlatBuffers nuget package tools folder