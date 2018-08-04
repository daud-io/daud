# Setting Up on a Mac

### Getting the Code

If you don't have github, create a github account
clone the repository https://github.com/andylippitt/IOGame

### Installing Tools

Visual Studio can be found at https://visualstudio.microsoft.com/vs/mac/

VSCode can be used as well

Make sure you install .net core, visual studio should prompt you to do so during the process.

### Opening and Running

Navigate to the code repository in finder
Double click "Game.Engine.sln"
If the project opens in Visual studio, that is good.

You can now run the project locally with the run button. 


### Possible Issues

If you get "error: the projet file cannot be found" when opening the .sln file, follow the steps below
 - navigate to the the Game.Engine folder.
 - double click the .csproj file
 - have it error
 - navigate to the the Game.Models folder.
 - double click the .csproj file
 - have it error
 - open the Game.Engine.sln file again
 - have it work (hopefully)

If the game does not load in a reasonable period of time, click the arrow next to where it says "debug" and change it to "release."

If you get "Unhandled Exception: Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.Internal.Networking.UvException: Error -13 EACCES permission denied" when running the game, try changing the port number in Program.cs