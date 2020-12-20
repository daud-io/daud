declare global {
    interface Window {
        dataLayer: any[];
    }
}

export class Events {
    static Report(category: string, action: string, value?: number): void {
        window.dataLayer = window.dataLayer || [];
        window.dataLayer.push("event", action, { event_category: category, value: value });
    }
    static Spawn(): void {
        Events.Report("life", "spawn");
    }

    static Death(secondsPlayed: number): void {
        Events.Report("life", "death", secondsPlayed);
    }

    static Spectate(): void {
        Events.Report("other", "spectate");
    }
}
