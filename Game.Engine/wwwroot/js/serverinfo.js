$(function () {
    var latencyDisplay = $('<li></li>');
    $('#ansiblelinks').append(latencyDisplay);


    var labelText = "thrust";

    var li = $('<li></li>');
    var label = $('<label></label>');
    li.append(label);

    var slider = $('<div class="attribute-slider"></div>');
    var update = function () {
        label.text(labelText + '(' + slider.slider('value') + ') : ');
    }
    slider.slider({
        min: 0,
        max: 20,
        slider: function (event, ui) {
            update();
        }
    });
    update();

    li.append(slider);


    $('#ansiblelinks').append(li);
    
    var MaxBoostTime = $('<li></li>');
    $('#ansiblelinks').append(MaxBoostTime);



    setInterval(function () {
        var connection = window.Game.primaryConnection;

        if (connection != null)
            latencyDisplay.text('ping: ' + (connection.latency ? (Math.floor(connection.latency * 100) / 100) + ' ms' : 'n/a'));
        else
            latencyDisplay.text('ping: n/a');

        if (Game.Hook) {
            //BaseThrust.text('base thrust: ' + (Game.Hook.BaseThrust));
            MaxBoostTime.text('max boost time: ' + (Game.Hook.MaxBoostTime));
        }
    }, 1000);
});
