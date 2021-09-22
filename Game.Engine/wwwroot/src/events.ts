import * as bus from "./bus";
import { Connection } from "./connection";
import { ServerWorld } from "./lobby";

let lastWorldKey = "";
let spawnTime = Date.now();
let connection: Connection | false = false;
let alive: number = 0;
let spawnCount: number = 0;

bus.on("connected", (con) => {
    connection = con;
});

bus.on("worldjoin", (worldKey: string, world: ServerWorld) => {
    lastWorldKey = worldKey;
});

bus.on("spawn", (name: string, ship: string) => {
    spawnTime = Date.now();
    (<any>window).dataLayer.push({
        event: "spawn",
        ship: ship,
        alive: alive,
        ping: connection ? connection.minimumLatency : null,
        cpu: connection ? connection.statViewCPUPerSecond : null,
        fps: connection ? connection.framesPerSecond : null,
        tx: connection ? connection.statBytesUpPerSecond : null,
        rx: connection ? connection.statBytesDownPerSecond : null,
        name: name,
        spawnCount: ++spawnCount,
    });
});

bus.on("dead", () => {
    alive += Date.now() - spawnTime;

    (<any>window).push({
        event: "dead",
        alive: alive,
        ping: connection ? connection.minimumLatency : null,
        cpu: connection ? connection.statViewCPUPerSecond : null,
        fps: connection ? connection.framesPerSecond : null,
        tx: connection ? connection.statBytesUpPerSecond : null,
        rx: connection ? connection.statBytesDownPerSecond : null,
    });
});

