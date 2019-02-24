# game player robots

you need the Game.Util https://github.com/daud-io/daud/tree/master/Game.Util

```
>game player robots --help
Usage: Game.Util player robots [options]

Options:
  --help                    Show help information
  -w|--world <WORLD>        World
  -r|--replicas <REPLICAS>  Replicas
  -f|--firing               Firing
  -n|--name <NAME>          Name
  -t|--target <TARGET>      Target
  -c|--color <COLOR>        Color
  -s|--sprite <SPRITE>      Sprite
  -v|--variation            Variation
  --type-name               TypeName
  --startup-delay           StartupDelay
  --file                    File
  --evolve                  Evolve
  ```



# examples
see: https://github.com/daud-io/daud/tree/master/Samples/Robots

```
game player robots --type-name Game.Robots.CTFBot,Game.Robots --world superduel
[1504041        robot]  Hooray, I'm alive!
```

# Robots

A robot is a program that acts as a player in the game. They see the world through Sensors 
and act by steering, shooting, and boosting.

# Sensors

`SensorTeam` Helps separate friend from foe. eg. robot.SensorTeam.IsSameTeam(unknownFleet)

`SensorFish` Describes the location of fish in view

`SensorFleets` Describes the location and makup of fleets in view

`SensorBullets` AHHH bullets!

`SensorAbandoned` see ships that were left behind after boosting

`SensorCTF` understand the state of play of Capture the Flag

# demo video
https://www.youtube.com/watch?v=pcd3S-bcs60&feature=youtu.be