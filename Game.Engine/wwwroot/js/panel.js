

var panelPinned = false;
(function () {
    document.querySelector('#panel .pin').addEventListener('click', function () {
        panelPinned = document.querySelector('#panel .pin').checked;
        if (panelPinned)
            document.querySelector('#panel').show();
        else
            document.querySelector('#panel').hide();
    });

    document.addEventListener('keydown', function (e) {
        if (e.keyCode == 70 || e.which == 70)
            document.querySelector('#panel').style.display = "block";
    });
    document.addEventListener('keyup', function (e) {
        if ((e.keyCode == 70 || e.which == 70) && !panelPinned)
            document.querySelector('#panel').style.display = "none";
    });
})();
