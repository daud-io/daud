
var popupShowing = false;

function bootstrapPopups()
{
    $('.change-log-button').on( 'click', function()
    {
        pressPopup('changelog');
    });

    $('#instructions').on( 'click', function()
    {
        pressPopup('instructions');
    });

    $('#changelogClose').on( 'click', function()
    {
        closePopup('changelog');
    });

    $('#instructionsClose').on( 'click', function()
    {
        closePopup('instructions');
    });

    $('#changelogBack').on( 'click', function()
    {
        closePopup('changelog');
    });

    $('#instructionsBack').on( 'click', function()
    {
        closePopup('instructions');
    });
}

function pressPopup(popupPressed)
{
    window.popupShowing = true;

    var popupToFadeIn = sortPopup(popupPressed);

    $(popupToFadeIn).fadeIn(500);
}

function closePopup(popupPressed)
{
    window.popupShowing = false;

    var popupToFadeOut = sortPopup(popupPressed);

    $(popupToFadeOut).fadeOut(500);
}

function sortPopup(popupPressed)
{
    return popupPressed = function()
    {
        switch(popupPressed)
        {
            case 'changelog':
                return $('#popupChangelog');
                break;
            case 'instructions':
                return $('#popupInstructions');
                break;
            default:
                break;
        }
    }();
}