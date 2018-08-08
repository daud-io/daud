$(function () {
    var latencyDisplay = $('<li></li>');
    $('#ansiblelinks').append(latencyDisplay);

    var attributes = [];

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

    attributes.push(buildAttribute("thrust", "BaseThrust", 0, 20, .5));
    attributes.push(buildAttribute("thrust(bot)", "BaseThrustBot", 0, 20, .5));

    attributes.push(buildAttribute("hit cost", "HealthHitCost", 0, 100, 1));
    attributes.push(buildAttribute("boost time", "MaxBoostTime", 0, 1000, 10));
    attributes.push(buildAttribute("health regen", "HealthRegenerationPerFrame", 0, 10, 0.3));

    attributes.push(buildAttribute("max speed", "MaxSpeed", 0, 100, 1));
    attributes.push(buildAttribute("max speed boost", "MaxSpeedBoost", 0, 100, 1));

    attributes.push(buildAttribute("shot cooldown", "ShootCooldownTime", 0, 5000, 1));
    attributes.push(buildAttribute("shot cool(bot)", "ShootCooldownTimeBot", 0, 5000, 1));

    attributes.push(buildAttribute("max health", "MaxHealth", 0, 500, 1));
    attributes.push(buildAttribute("max health(bot)", "MaxHealthBot", 0, 500, 1));
    
    
    setInterval(function () {
        var connection = window.Game.primaryConnection;

        if (connection != null)
            latencyDisplay.text('ping: ' + (connection.latency ? (Math.floor(connection.latency * 100) / 100) + ' ms' : 'n/a'));
        else
            latencyDisplay.text('ping: n/a');

        if (Game.Hook) {
            for (var i = 0; i < attributes.length; i++) {
                attributes[i].update(Game.Hook);
            }
        }
    }, 1000);
});
