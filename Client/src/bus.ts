import { Connection, LeaderboardType } from "./connection";
import { NetWorldView } from "./daud-net/net-world-view";
import { ServerWorld } from "./registry";

const events = {
    gameReady: [] as (() => void)[],
    pageReady: [] as (() => void)[],
    worldjoin: [] as ((connect: string, world: ServerWorld) => void)[],
    connected: [] as ((connection: Connection) => void)[],
    dead: [] as (() => void)[],
    spawn: [] as ((name: string, ship: string) => void)[],
    settings: [] as (() => void)[],
    leaderboard: [] as ((leaderboard: LeaderboardType) => void)[],
    loaded: [] as (() => void)[],
    hook: [] as ((hook: any) => void)[],
    themechange: [] as (() => void)[],
    worldview: [] as ((worldview: NetWorldView) => void)[],
};

type Magic = any; // The type is valid, but typescript can't understand that yet

function emit<T extends keyof typeof events>(event: T, ...args: Parameters<typeof events[T][number]>): void {
    for (const i of events[event]) {
        (i as Magic)(...args);
    }
}

function on<T extends keyof typeof events>(event: T, cb: typeof events[T][number]): () => void {
    (events[event] as Magic).push(cb);
    return () => (events[event] = (events[event] as Magic).filter((x) => x !== cb));
}

export { on, emit };
