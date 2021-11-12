import { Connection, LeaderboardType } from "./connection";
import { NetWorldView } from "./daud-net/net-world-view";

const events = {
    gameReady: [] as (() => void)[],
    pageReady: [] as (() => void)[],
    worldjoin: [] as ((connect: string) => void)[],
    connected: [] as ((connection: Connection) => void)[],
    disconnected: [] as (() => void)[],
    connectionError: [] as (() => void)[],
    dead: [] as (() => void)[],
    alive: [] as (() => void)[],
    spawn: [] as ((name: string, ship: string) => void)[],
    settings: [] as (() => void)[],
    leaderboard: [] as ((leaderboard: LeaderboardType) => void)[],
    loaded: [] as (() => void)[],
    hook: [] as ((hook: any) => void)[],
    themechange: [] as (() => void)[],
    worldview: [] as ((worldview: NetWorldView) => void)[],
    prerender: [] as ((time: number) => void)[],
    postrender: [] as ((time: number) => void)[]

};

type Magic = any; // The type is valid, but typescript can't understand that yet

function emit<T extends keyof typeof events>(event: T, ...args: Parameters<typeof events[T][number]>): void {
    for (const i of events[event]) {
        (i as Magic)(...args);
    }
}

function emitWorldview(worldview: NetWorldView): void {
    for (let i=0; i<events['worldview'].length; i++)
        events['worldview'][i](worldview);
}
function emitPrerender(time: number): void {
    for (let i=0; i<events['prerender'].length; i++)
        events['prerender'][i](time);
}
function emitPostrender(time: number): void {
    for (let i=0; i<events['postrender'].length; i++)
        events['postrender'][i](time);
}

function on<T extends keyof typeof events>(event: T, cb: typeof events[T][number]): () => void {
    (events[event] as Magic).push(cb);
    return () => (events[event] = (events[event] as Magic).filter((x) => x !== cb));
}

export { on, emit, emitWorldview, emitPrerender, emitPostrender };


