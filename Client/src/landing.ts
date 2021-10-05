import * as bus from "./bus";
import { PingConnection } from "./pingconnection";
import { Host, Registry, ServerWorld } from './registry';

export class Landing {
    static worlds: Record<string, ServerWorld> = {};

    static connections: Record<string, PingConnection> = {};
    static history: Record<string, number> = {};
    static timer: number;

    static visible: boolean = true;
    static lastRegistryCheck: number = 0;

    static currentConnect: string;

    static gameLoaded: boolean = false;

    static pingEnabled: boolean = false;
    static touchWarned: boolean = false;
    static firstLoad: boolean = true;;

    static tryAddConnection(host: Host): void {
        if (!this.pingEnabled)
            return;

        if (!this.connections[host.url] && !this.history[host.url]) {
            const connect = `${host.url}/${host.worlds[0].worldKey}`;
            const connection = new PingConnection();
            connection.connect(connect);

            this.connections[host.url] = connection;
        }
    }

    static touchwarn() {
        if (!this.touchWarned) {
            this.touchWarned = true;

            setTimeout(() => {
                document.getElementById('touchwarn')?.classList.remove('closed');
            }, 500);
        }
    }

    static async checkRegistry(): Promise<void> {
        const hosts = await Registry.query();
        hosts.forEach((host) => {
            if (host.worlds.length > 0) {
                this.tryAddConnection(host);
            }

            host.worlds.forEach(world => {
                const connect = `${host.url}/${world.worldKey}`;
                this.worlds[connect] = world;

                document.querySelectorAll(`.${host.url.replaceAll('.', '-')}-${world.worldKey}-players`).forEach(e => {
                    e.innerHTML = `${world.advertisedPlayers}`;

                    e.closest('.world')?.classList.remove('pending');
                });
            });
        });
    }

    static checkHosts(): void {
        console.log('checkhosts');
        if (!this.pingEnabled || !this.visible)
            return;

        for (let hostname in this.connections) {
            const connection = this.connections[hostname];

            if (connection.minimumLatency > -1)
                this.history[hostname] = connection.minimumLatency;

            this.updateHost(hostname, connection.minimumLatency);
        }

        for (let hostname in this.history)
            if (!this.connections[hostname])
                this.updateHost(hostname, this.history[hostname])

        if (performance.now() - this.lastRegistryCheck > 5000) {
            this.lastRegistryCheck = performance.now();
            console.log('checkhosts: checkRegistry');
            this.checkRegistry();
        }

    }

    static updateHost(hostname: string, latency: number): void {
        document.querySelectorAll(`.${hostname.replaceAll('.', '-')}-ping`).forEach(e => {
            var worldClasses = e.closest(".world")?.classList;
            if (worldClasses) {
                var bucket: string = latency == -1 ? 'pending' :
                    latency < 50 ? 'fast' :
                        latency < 120 ? 'medium' :
                            'slow'

                if (!worldClasses.contains(bucket)) {
                    worldClasses.remove('fast', 'medium', 'slow', 'pending');
                    worldClasses.add(bucket);
                }

                e.innerHTML = `${bucket == 'pending' ? '--' : bucket} [${latency == -1 ? '--' : Math.floor(latency)}ms]`;
            }
        });
    }

    static clearHosts(): void {
        for (let connect in this.connections)
            this.connections[connect]?.disconnect();

        this.connections = {};
    }

    static show() {
        if (!this.firstLoad) {
            this.pingEnabled = true;
            document.getElementById('worlds')?.classList.add('pingenabled');
        }

        this.firstLoad = false;

        this.checkRegistry();
        window.document.body.classList.add('landing');
        this.setElementDisplay('gameArea', 'none');
        this.setElementDisplay('spawnscreen', 'none');
        this.setElementDisplay('lobby', '');
        this.visible = true;
    }

    static hide() {
        window.document.body.classList.remove('landing');
        this.setElementDisplay('gameArea', '');
        this.setElementDisplay('spawnscreen', '');
        this.setElementDisplay('lobby', 'none');
        this.visible = false;
    }

    private static setElementDisplay(id: string, styleDisplayValue: string) {
        const element = document.getElementById(id)
        if (element)
            element.style.display = styleDisplayValue;
    }

    private static async launch(connect: string): Promise<void> {
        this.hide();

        this.clearHosts();
        if (this.currentConnect != connect) {
            this.currentConnect = connect;
            if (!this.gameLoaded) {
                bus.on('gameReady', () => {
                    this.gameLoaded = true;
                    bus.emit("worldjoin", connect, this.worlds[connect]);
                });

                await import('./game');
            }
            else
                bus.emit("worldjoin", connect, this.worlds[connect]);

        }
    }

    private static onArenasClick(e: Event) {
        this.show();
    }

    private static onWorldClick(e: Event) {
        const world = (<HTMLElement>(e.target))?.closest('.world');
        const connect = world?.attributes.getNamedItem('data-connect')?.value;

        console.log('world click');
        e.preventDefault();

        if (connect)
            this.launch(connect);

    }

    static async initialize(): Promise<void> {
        try {
            this.timer = window.setInterval(() => this.checkHosts(), 500);

            document.body.classList.add('dead');

            document.getElementById('worlds')?.addEventListener("touchend", () => this.touchwarn());
            document.getElementById('worlds')?.addEventListener("click", (e) => this.onWorldClick(e));
            document.getElementById('arenas')?.addEventListener("click", (e) => this.onArenasClick(e));

            this.show();

            document.getElementById("touchwarn")!.addEventListener("click", () => {
                document.getElementById('touchwarn')?.classList.add('closed');
            });
            document.getElementById("touchwarnClose")!.addEventListener("click", () => {
                document.getElementById('touchwarn')?.classList.add('closed');
            });
        }
        catch (e) {
            console.log(e);
        }
    }
}

Landing.initialize();

