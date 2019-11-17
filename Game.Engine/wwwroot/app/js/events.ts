export class Events {
    static Report(category, action, value?) {
        (<any>window).dataLayer = (<any>window).dataLayer || [];
        (<any>window).dataLayer.push("event", action, { event_category: category, value: value });
    }
    static Spawn() {
        Events.Report("life", "spawn");
    }

    static Death(secondsPlayed) {
        Events.Report("life", "death", secondsPlayed);
    }

    static Spectate() {
        Events.Report("other", "spectate");
    }

    static changeRoom(room) {
        Events.Report("room", room);
    }
}
