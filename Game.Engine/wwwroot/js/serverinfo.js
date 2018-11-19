import { Cache } from "./cache";

var latencyDisplay = document.createElement("li");
document.querySelector("#ansiblelinks").append(latencyDisplay);

var bandwidthDisplay = document.createElement("li");
document.querySelector("#ansiblelinks").append(bandwidthDisplay);

var statsDisplay = document.createElement("li");
document.querySelector("#ansiblelinks").append(statsDisplay);

var attributes = [];

var buildAttributeToggle = (labelText, propertyName) => {
    var li = document.createElement("li");
    var label = document.createElement("label");
    li.append(label);

    var check = document.createElement("input");
    check.type = "checkbox";
    var draw = value => {
        value = value || check.checked;
        label.innerText = `${labelText}(${value}) : `;
    };

    check.addEventListener("change", event => {
        if (Game && Game.Hook) {
            Game.Hook[propertyName] = check.prop("checked");
            Game.Hook.New = true;
        }
        draw();
    });

    draw();

    li.append(check);
    document.querySelector("#ansiblelinks").append(li);

    var updater = {
        update(hook) {
            check.checked = hook[propertyName];
            draw();
        }
    };
    return updater;
};

var buildAttribute = (labelText, propertyName, min, max, step) => {
    var li = document.createElement("li");
    var label = document.createElement("label");
    li.append(label);

    var slider = document.createElement("input");
    slider.type = "range";
    slider.classList.add("attribute-slider");
    var draw = value => {
        value = value || slider.value;

        label.innerText = `${labelText}(${value}) : `;
    };
    slider.min = min;
    slider.max = max;
    slider.value = min;
    slider.step = step;
    slider.addEventListener("change", () => {
        if (Game && Game.Hook) {
            Game.Hook[propertyName] = slider.value;
            Game.Hook.New = true;
            //console.log(Game.Hook);
        }
        draw();
    });
    // slider.slider({
    //     min: min,
    //     max: max,
    //     value: min,
    //     step: step,
    //     slide: function (event, ui) {

    //         if (Game && Game.Hook) {
    //             Game.Hook[propertyName] = ui.value;
    //             Game.Hook.New = true;
    //             //console.log(Game.Hook);
    //         }
    //         draw();
    //     }
    // });
    draw();

    li.append(slider);
    document.querySelector("#ansiblelinks").append(li);

    var updater = {
        update(hook) {
            slider.value = hook[propertyName];
            draw();
        }
    };
    return updater;
};

attributes.push(buildAttribute("thrust", "BaseThrust", 0, 1, 0.025));
attributes.push(buildAttribute("thrust(bot)", "BaseThrustBot", 0, 1, 0.025));

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

setInterval(() => {
    var connection = window.Game.primaryConnection;

    statsDisplay.innerText = `vps:${window.Game.Stats.viewsPerSecond} ups:${window.Game.Stats.updatesPerSecond} fps:${window.Game.Stats.framesPerSecond} cs:${Cache.count}`;

    if (connection !== null) {
        bandwidthDisplay.innerText = `bandwidth: ${(Math.floor(connection.statBytesUpPerSecond / 102.4) / 10) * 8}Kb/s up ${(Math.floor(connection.statBytesDownPerSecond / 102.4) / 10) * 8}Kb/s down`;
        latencyDisplay.innerText = `ping: ${connection.latency ? Math.floor(connection.latency * 100) / 100 + " ms" : "n/a"}`;
    } else latencyDisplay.innerText = "ping: n/a";

    if (Game.Hook) {
        for (let i = 0; i < attributes.length; i++) {
            attributes[i].update(Game.Hook);
        }
    }
}, 1000);
