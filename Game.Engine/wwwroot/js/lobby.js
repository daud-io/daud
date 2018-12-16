import { fetch } from "whatwg-fetch";
import { Controls } from "./controls";

export var Lobby = {
    allWorlds: {},
    connection: false,
    refreshList: function()
    {
        fetch("/api/v1/server/worlds", {
            method: "GET",
            headers: {
                "Content-Type": "application/json; charset=utf-8",
            }
        })
            .then(r => r.json())
            .then(({ success, response }) => {
                if (success) {
                    var selector = document.getElementById('worldSelector');
                    var selected = selector.value;
                    if (!selected)
                    {
                        if (window.location.hash)
                            selected = window.location.hash.substring(1);
                        else
                            selected = "default";
                    }

                    var options = '';
                    Lobby.allWorlds = {};
                    
                    for(var i=0; i<response.length; i++)
                    {
                        var world = response[i];
                        Lobby.allWorlds[world.world] = world;

                        options += `<option value="${world.world}">${world.players}: ${world.name}</option>`;
                    }

                    selector.innerHTML = options;
                    selector.value = selected;
            
                    Lobby.showDescription(selected);
                }
            });
    },
    showDescription: function(worldKey)
    {
        var world = Lobby.allWorlds[worldKey];
        if (world)
            document.getElementById('arenaDescription').innerHTML = world.description;
        else
            document.getElementById('arenaDescription').innerHTML = worldKey;
    },
    changeRoom: function(worldKey) 
    {
        var world = Lobby.allWorlds[worldKey];
        if (world)
        {
            var colors = world.allowedColors;
            var options = '';

            for(var i=0; i<colors.length; i++)
                options += `<option value="${colors[i]}">${colors[i]}</option>`;

            document.getElementById("shipSelector").innerHTML = options;
            document.getElementById("shipSelector").value = colors[0];
            Controls.color = colors[0];


            Lobby.showDescription(worldKey);

        }
        else
            console.log(`Warning: could not find selected world ${worldKey}`);
    }
};

Lobby.refreshList();
setInterval(Lobby.refreshList, 3000);


