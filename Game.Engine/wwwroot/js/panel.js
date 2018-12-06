var toggle = false;

document.addEventListener("keydown", function(e) {
    if ((e.keyCode == 70 || e.which == 70)) {

        if (toggle)
        {
            document.getElementById('titan').style.visibility = 'visible';
        }
        else
        {
            document.getElementById('titan').style.visibility = 'hidden';
        }

        toggle = !toggle;

        pressable = false;
    }
});
