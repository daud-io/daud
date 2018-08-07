$(function () {
    var latencyDisplay = $('<li></li>');
    $('#ansiblelinks').append(latencyDisplay);

    var BaseThrust = $('<li></li>');
    $('#ansiblelinks').append(BaseThrust);
    var MaxBoostTime = $('<li></li>');
    $('#ansiblelinks').append(MaxBoostTime);

    setInterval(function () {
        var connection = window.Game.primaryConnection;

        if (connection != null)
            latencyDisplay.text('ping: ' + (connection.latency ? (Math.floor(connection.latency * 100) / 100) + ' ms' : 'n/a'));
        else
            latencyDisplay.text('ping: n/a');

        if (Game.Hook) {
            BaseThrust.text('base thrust: ' + (Game.Hook.BaseThrust));
            MaxBoostTime.text('max boost time: ' + (Game.Hook.MaxBoostTime));
        }
    }, 1000);
});
