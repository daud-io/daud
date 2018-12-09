export class Events {
    static Report(category, action) {
        if (!ga)
            return;

        try {
            ga('send', {
                'hitType': 'event',
                'eventCategory': category,
                'eventAction': action,
                'hitCallback': function () {
                }
            });
        }
        catch (e) {
            console.log("exception in Events.Report");
            console.log(e);
        }
    }

    static Spawn() {
        Events.Report('life', 'spawn');
    }

    static Death() {
        Events.Report('life', 'death');
    }

    static Spectate() {
        Events.Report('other', 'spectate');
    }

    static ChangeRoom(room) {
        Events.Report('room', room);
    }
}
