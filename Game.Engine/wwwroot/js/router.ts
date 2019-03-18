import Cookies from "js-cookie";
import { Connection } from "./connection";

export class Router {
    bestServer: string;
    allResults: any[];

    public constructor() {
        this.load();
    }

    private load()
    {
        const savedRouterConfig = Cookies.getJSON("router");
        if (savedRouterConfig) {
            this.bestServer = savedRouterConfig.bestServer;
        }
    }

    private save()
    {
        Cookies.set("router", {}, { expires: 300 });
    }

    public findBestServer(servers: string[])
    {
        for (var server in servers)
            this.pingServer(server);

        setTimeout(function() {
            
        }, 1100);
    }

    public pingServer(worldKey: string)
    {
        var connection = new Connection();
        connection.bandwidthThrottle = 0;
        connection.autoReload = false;
        connection.connect(worldKey);

        var self = this;
        setTimeout(function() {
            if (connection.connected && connection.statPongCount > 1)
                self.allResults.push({ worldKey: worldKey, latency: connection.latency });
        }, 1000);
    }
}
