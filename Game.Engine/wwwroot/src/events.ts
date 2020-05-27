﻿export class Events {
    static Report(category: string, action: string, value?: number) {
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
}
