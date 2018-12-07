var isVisible = false;

function hide() {
    document.getElementById('titan').style.visibility = 'hidden';
    isVisible = false;
}

function show() {
    document.getElementById('titan').style.visibility = 'visible';
    document.getElementById('titan-frame').focus();
    isVisible = true;
}

document.addEventListener("keydown", function(e) {
    if ((e.keyCode == 70 || e.which == 70)
        && (
            document.body.classList.contains("alive")
        || document.body.classList.contains("spectating")
        )
    ) {

        if (!isVisible)
            show();
        else
            hide();
    }
});

document.addEventListener("mousedown", function (e) {
    if (isVisible)
        hide();
});