# Daud.io - game
https://daud.io

# Deploying to heroku
[![Deploy](https://www.herokucdn.com/deploy/button.svg)](https://heroku.com/deploy?template=https://github.com/daud-io/daud)

# Requirements
1. Microsoft dotnet core 5.0 SDK - https://dotnet.microsoft.com/download
2. Node js - https://nodejs.org/en/

# Suggestions
1. Github Desktop - https://desktop.github.com/
2. VS Code - https://code.visualstudio.com/

### Running from the command line

1. `git clone https://github.com/daud-io/daud.git`
2. `cd daud/Game.Engine/wwwroot`
3. `npm install`
4. `npm run build`
5. `cd ..`
6. `dotnet run` or `dotnet run -c Release`

This should start a web server at http://localhost:5000
open Game Modes and look for your local server rooms at the bottom of the list

### Publishing to Heroku
1. Set up an account on Heroku and on Docker Hub
2. `./deploy.sh`

If you get an error that the file './deploy.sh' is not executable by this user: run `chmod +x ./deploy.sh`

### Getting the Code

If you don't have github, create a github account
clone the repository https://github.com/daud-io/daud

### Opening and Running

Navigate to the code repository in finder
Double click "Game.Engine.sln"
If the project opens in Visual studio, that is good.

You can now run the project locally with the run button. 
