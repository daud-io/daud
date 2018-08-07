$(function () {
    var latencyDisplay = $('<li></li>');
    $('#ansiblelinks').append(latencyDisplay);

    setInterval(function () {
        var connection = window.Game.primaryConnection;

        if (connection != null)
            latencyDisplay.text('ping: ' + (connection.latency ? (Math.floor(connection.latency * 100) / 100) + ' ms' : 'n/a'));
        else
            latencyDisplay.text('ping: n/a');
    }, 1000);
});
