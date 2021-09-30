import * as bus from "./bus";
import { Connection } from "./connection";

let spawnTime = Date.now();
let connection: Connection | false = false;
let alive: number = 0;
let spawnCount: number = 0;

(<any>window).dataLayer = (<any>window).dataLayer || [];

bus.on("connected", (con) => {
    connection = con;
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

    (<any>window).dataLayer.push({
        event: "dead",
        alive: alive,
        ping: connection ? connection.minimumLatency : null,
        cpu: connection ? connection.statViewCPUPerSecond : null,
        fps: connection ? connection.framesPerSecond : null,
        tx: connection ? connection.statBytesUpPerSecond : null,
        rx: connection ? connection.statBytesDownPerSecond : null,
    });
});

