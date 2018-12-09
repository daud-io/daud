var isVisible = false;
var isFirst = true;

function hide() {
    document.getElementById('titan').style.display = 'none';
    document.getElementById('titan').style.visibility = 'hidden';
    isVisible = false;
}

function show() {
    document.getElementById('titan').style.display = 'block';
    document.getElementById('titan').style.visibility = 'visible';
    if (isFirst) {
        document.getElementById('titan-frame').src = 'https://titanembeds.com/embed/472025150959648791?css=388&defaultchannel=520343898347012097';
        isFirst = false;

    }
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