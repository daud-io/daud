$(function () {
    var latencyDisplay = $('<li></li>');
    $('#ansiblelinks').append(latencyDisplay);

    var bandwidthDisplay = $('<li></li>');
    $('#ansiblelinks').append(bandwidthDisplay);

    var statsDisplay = $('<li></li>');
    $('#ansiblelinks').append(statsDisplay);

    var attributes = [];

    var buildAttributeToggle = function (labelText, propertyName) {
        var li = $('<li></li>');
        var label = $('<label></label>');
        li.append(label);

        var check = $('<input type="checkbox" />');
        var draw = function (value) {
            value = value || check.prop('checked');
            label.text(labelText + '(' + (value) + ') : ');
        };

        check.on('change', function (event) {
            if (Game && Game.Hook) {
                Game.Hook[propertyName] = check.prop('checked');
                Game.Hook.New = true;
            }
            draw();
        });

        draw();

        li.append(check);
        $('#ansiblelinks').append(li);

        var updater = {
            update: function (hook) {
                check.prop('checked', hook[propertyName]);
                draw();
            }
        };
        return updater;
    }


    var buildAttribute = function(labelText, propertyName, min, max, step)
    {
        var li = $('<li></li>');
        var label = $('<label></label>');
        li.append(label);

        var slider = $('<div class="attribute-slider"></div>');
        var draw = function (value) {
            value = value || slider.slider('value');

            label.text(labelText + '(' + (value) + ') : ');
        };

        slider.slider({
            min: min,
            max: max,
            value: min,
            step: step,
            slide: function (event, ui) {

                if (Game && Game.Hook) {
                    Game.Hook[propertyName] = ui.value;
                    Game.Hook.New = true;
                    //console.log(Game.Hook);
                }
                draw();
            }
        });
        draw();

        li.append(slider);
        $('#ansiblelinks').append(li);

        var updater = {
            update: function (hook) {
                slider.slider('value', hook[propertyName]);
                draw();
            }
        };
        return updater;
    }

    /*
    attributes.push(buildAttribute("thrust", "BaseThrust", 0, 1, .025));
    attributes.push(buildAttribute("thrust(bot)", "BaseThrustBot", 0, 1, .025));

    attributes.push(buildAttribute("hit cost", "HealthHitCost", 0, 100, 1));
    attributes.push(buildAttribute("boost time", "MaxBoostTime", 0, 1000, 10));
    attributes.push(buildAttribute("health regen", "HealthRegenerationPerFrame", 0, 2, 0.1));

    attributes.push(buildAttribute("max speed", "MaxSpeed", 0, 3, 0.2));
    attributes.push(buildAttribute("max speed boost", "MaxSpeedBoost", 0, 3, 0.2));

    attributes.push(buildAttribute("shot cooldown", "ShootCooldownTime", 0, 5000, 1));
    attributes.push(buildAttribute("shot cool(bot)", "ShootCooldownTimeBot", 0, 5000, 1));

    attributes.push(buildAttribute("max health", "MaxHealth", 0, 500, 1));
    attributes.push(buildAttribute("max health(bot)", "MaxHealthBot", 0, 500, 1));

    attributes.push(buildAttribute("bullet speed", "BulletSpeed", 0, 3, 0.1));
    attributes.push(buildAttribute("bullet life", "BulletLife", 0, 25000, 1));

    attributes.push(buildAttribute("bot per X", "BotPerXPoints", 100, 20000, 1));
    attributes.push(buildAttribute("bot base", "BotBase", 0, 25, 1));

    attributes.push(buildAttribute("obstacles", "Obstacles", 0, 25, 1));

    attributes.push(buildAttributeToggle("team mode", "TeamMode"));

    attributes.push(buildAttribute("flocking", "FlockWeight", 0, 0.2, 0.01));
    attributes.push(buildAttribute("flocking pseed", "FlockSpeed", 0, 1000, 40));
    attributes.push(buildAttribute("cohesion", "FlockCohesion", 0, 0.005, 0.00001));
    attributes.push(buildAttribute("separation", "FlockSeparation", 0, 40, 0.01));
    attributes.push(buildAttribute("sep  dist", "FlockSeparationMinimumDistance", 0, 400, 20));
    attributes.push(buildAttribute("alignment", "FlockAlignment", 0, 0.2, 0.01));
    */
    
    setInterval(function () {
        var connection = window.Game.primaryConnection;

        statsDisplay.text(
            'vps:' + Game.Stats.viewsPerSecond +
            ' ups:' + Game.Stats.updatesPerSecond +
            ' fps:' + Game.Stats.framesPerSecond
        );

        if (connection !== null) {
            bandwidthDisplay.text('bandwidth: '
                + Math.floor(connection.statBytesUpPerSecond / 102.4) / 10 * 8 + 'Kb/s up '
                + Math.floor(connection.statBytesDownPerSecond / 102.4) / 10 * 8 + 'Kb/s down'
            );
            latencyDisplay.text('ping: ' + (connection.latency ? (Math.floor(connection.latency * 100) / 100) + ' ms' : 'n/a'));
        }
        else
            latencyDisplay.text('ping: n/a');

        if (Game.Hook) {
            for (var i = 0; i < attributes.length; i++) {
                attributes[i].update(Game.Hook);
            }
        }
    }, 1000);
});
