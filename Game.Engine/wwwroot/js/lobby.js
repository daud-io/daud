                    }

                    selector.innerHTML = options;
                    selector.value = selected;
                }
            });
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

            document.getElementById('arenaDescription').innerHTML = world.description;

        }
        else
            console.log(`Warning: could not find selected world ${worldKey}`);
        /*
                document.getElementById("shipSelector").innerHTML = '<option selected value="cyan">cyan</option>' + '<option value="red">red</option>';
                Controls.ship = "ship_cyan";
                Controls.color = "cyan";
    
                break;
            default:
                document.getElementById("shipSelector").innerHTML =
                    '<option value="green">green</option>' +
                    '<option value="orange">orange</option>' +
                    '<option value="pink">pink</option>' +
                    '<option value="red">red</option>' +
                    '<option value="cyan">cyan</option>' +
                    '<option value="yellow">yellow</option>';
                break;
        }        */
    }
};

Lobby.refreshList();
setInterval(Lobby.refreshList, 3000);


