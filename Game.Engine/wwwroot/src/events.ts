export class Events {
    static Report(category, action, value?) {
        (window as any).dataLayer = (window as any).dataLayer || [];
        (window as any).dataLayer.push("event", action, { event_category: category, value: value });
    }
    static Spawn() {
        Events.Report("life", "spawn");
    }

    static Death(secondsPlayed: number) {
        Events.Report("life", "death", secondsPlayed);
    }

    static Spectate() {
        Events.Report("other", "spectate");
    }

    static changeRoom(room) {
        Events.Report("room", room);
    }
}
