export class Events {
    static Report(category, action, value) {
        window.dataLayer = window.dataLayer || [];
        function gtag(...args) {
            dataLayer.push(args);
        }
        gtag("event", action, { event_category: category, value });
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
