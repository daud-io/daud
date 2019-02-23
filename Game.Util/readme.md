# Setup of `game` util
The Game.Util project is a CLI interface for the game API.

build the code from the Game.Util folder

```
C:\code\IOGame\Game.Util>dotnet build
Microsoft (R) Build Engine version 15.8.166+gd4e8d81a88 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  Restoring packages for C:\code\IOGame\Game.API.Common\Game.API.Common.csproj...
  Restoring packages for C:\code\IOGame\Game.API.Client\Game.API.Client.csproj...
  Restoring packages for C:\code\IOGame\Game.Robots\Game.Robots.csproj...
  Restoring packages for C:\code\IOGame\Game.Util\Game.Util.csproj...
  Restore completed in 89.75 ms for C:\code\IOGame\Game.Engine.Networking.FlatBuffers\Game.Engine.Networking.FlatBuffers.csproj.
  Generating MSBuild file C:\code\IOGame\Game.API.Common\obj\Game.API.Common.csproj.nuget.g.props.
  Generating MSBuild file C:\code\IOGame\Game.API.Common\obj\Game.API.Common.csproj.nuget.g.targets.
  Restore completed in 965.94 ms for C:\code\IOGame\Game.API.Common\Game.API.Common.csproj.
  Generating MSBuild file C:\code\IOGame\Game.API.Client\obj\Game.API.Client.csproj.nuget.g.props.
  Generating MSBuild file C:\code\IOGame\Game.API.Client\obj\Game.API.Client.csproj.nuget.g.targets.
  Restore completed in 2.28 sec for C:\code\IOGame\Game.API.Client\Game.API.Client.csproj.
  Installing GeneticSharp 2.5.1.
  Installing TiledSharp 1.0.1.
  Restore completed in 2.55 sec for C:\code\IOGame\Game.Robots\Game.Robots.csproj.
  Restore completed in 2.55 sec for C:\code\IOGame\Game.Util\Game.Util.csproj.
C:\Program Files\dotnet\sdk\2.1.401\Sdks\Microsoft.NET.Sdk\targets\Microsoft.PackageDependencyResolution.targets(198,5): message NETSDK1062: Unable to use package assets cache due to I/O error. This can occur when the same project is built more than once in parallel. Performance may be degraded, but the build result will not be impacted. [C:\code\IOGame\Game.Robots\Game.Robots.csproj]
  Game.API.Common -> C:\code\IOGame\Game.API.Common\bin\Debug\netcoreapp2.1\Game.API.Common.dll
  Game.Engine.Networking.FlatBuffers -> C:\code\IOGame\Game.Engine.Networking.FlatBuffers\bin\Debug\netcoreapp2.1\Game.Engine.Networking.FlatBuffers.dll
  Game.API.Client -> C:\code\IOGame\Game.API.Client\bin\Debug\netcoreapp2.1\Game.API.Client.dll
  Game.Robots -> C:\code\IOGame\Game.Robots\bin\Debug\netcoreapp2.1\Game.Robots.dll
  Game.Util -> C:\code\IOGame\Game.Util\bin\Debug\netcoreapp2.1\Game.Util.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.63

C:\code\IOGame\Game.Util>
```


# windows specific
see scripts/game.cmd (todo link)
copy that to someplace in your path, eg. c:\windows

# bash equiv
todo - add scripts/game.sh

# execute
```
C:\code\IOGame\Game.Util>game --help
Usage: Game.Util [options] [command]

Options:
  --help                    Show help information
  --context                 override the default, saved context and use the mentioned one
  -s|--server <SERVER>      full url of the Game API server
  -u|--user-key <USER_KEY>  specify a UserKey for authentication
  -p|--password <PASSWORD>  spefify a password for authentication
  --registry-server         full url of the Registry API server
  --registry-user-key       specify a UserKey for registry authentication
  --registry-password       specify a Password for registry authentication
  -t|--token <TOKEN>        specify a token for authentication

Commands:
  context
  player
  registry
  server
  world

Run 'Game.Util [command] --help' for more information about a command.
```

# Basic usage
```
C:\code\IOGame\Game.Util>game --server http://us.daud.io --user-key Administrator --password N0tTh3Re41.n3 server get
==== Server ====
Server              ms        Players
-------------------------------------
http://us.daud.io/  940.1844  18
```
# Contexts

you can save the credentials and urls to a config file called a context in your user profile directory

example:
c:\users\littledr\.game\config.json (or linux/mac ~/.game/config.json)
```
{
  "CurrentContext": "us.daud.io",
  "Contexts": {
    "us.daud.io": {
      "Uri": "https://us.daud.io/",
      "UserKey": "Administrator",
      "Password": "N0tTh3Re41.n3",
      "RegistryUri": null,
      "RegistryUserKey": null,
      "RegistryPassword": null
    },
    "local": {
      "Uri": "http://localhost:5000/",
      "UserKey": "Administrator",
      "Password": "",
      "RegistryUri": null,
      "RegistryUserKey": null,
      "RegistryPassword": null
    }
  }
}
```

# game contexts
```
C:\code\IOGame\Game.Util>game context list
==== Contexts ====
IsCurrent  Key
-----------------
*          us.daud.io
           local
```

```
C:\code\IOGame\Game.Util>game context set us.daud.io
```
Now the equivalent of the above config with context
```
C:\code\IOGame\Game.Util>game server get
==== Server ====
Server               ms         Players
---------------------------------------
https://us.daud.io/  1356.3904  14
```

