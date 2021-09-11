import Cookies from "js-cookie";
import { Connection } from "./connection";

type Server = { worldKey: string; latency: number };

const savedRouterConfig = JSON.parse(Cookies.get("router") || "false");
export const bestServer: string = savedRouterConfig?.bestServer;
let allResults: Server[];

export function save(server: string): void {
    //Stores a cookie of the best found server.
    //Set to expire every 7 days.
    Cookies.set("router", { bestServer: server }, { expires: 7 });
}

export function findBestServer(servers: string[], next: (bestServer: string) => void): void {
    allResults = [];

    servers.forEach((server) => {
        pingServer(server);
    });

    setTimeout(() => {
        let best: Server;

        allResults.forEach((result) => {
            if (!best || result.latency < best.latency) best = result;
        });

        next(best!.worldKey);
    }, 2500);
}
function pingServer(worldKey: string): void {
    const connection = new Connection();
    connection.bandwidthThrottle = 1;
    connection.autoReload = false;
    connection.connect(worldKey);

    setTimeout(() => {
        //console.log({ worldKey: worldKey, latency: connection.latency, connected: connection.connected, pongs: connection.statPongCount });
        if (connection.connected && connection.statPongCount > 1) allResults.push({ worldKey: worldKey, latency: connection.latency });

        connection.disconnect();
    }, 1000);
}
