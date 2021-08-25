import { LeaderboardType } from "./connection";
import { ServerWorld } from "./lobby";

const events = {
    worldjoin: [] as ((worldKey: string, world: ServerWorld) => void)[],
    dead: [] as (() => void)[],
    settings: [] as (() => void)[],
    leaderboard: [] as ((leaderboard: LeaderboardType) => void)[],
    loaded: [] as (() => void)[],
    hook: [] as ((hook: any) => void)[],
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

export default { on, emit };
