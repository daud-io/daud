import './fills';
import * as bus from "./bus";
import { Pinger } from './pinger';
import { Host, Registry, ServerWorld } from './registry';
import './game';
import { html, render, Hole, Tag } from "uhtml";
import './pwa';
export class Landing {
    worlds: Record<string, ServerWorld> = {};

    connections: Record<string, Pinger> = {};
    history: Record<string, number> = {};
    timer: number = -1;

    visible: boolean = true;
    lastRegistryCheck: number = 0;

    currentConnect: string|undefined = undefined;

    gameLoaded: boolean = true;

    pingEnabled: boolean = false;
    firstLoad: boolean = true;

    entireUIHidden: boolean = true;

    primaryServers = ['us.daud.io', 'de.daud.io'];

    constructor()
    {
        this.initialize();
        bus.on('connectionError', () => {
            this.currentConnect = undefined;
            this.show();
        });
    }

    tryAddConnection(host: Host): void {
        if (!this.pingEnabled)
            return;

        if (!this.connections[host.url] && !this.history[host.url]) {
            const connect = `${host.url}/${host.worlds[0].worldKey}`;
            const connection = new Pinger();
            connection.connect(connect);

            this.connections[host.url] = connection;
        }
    }

    async checkRegistry(): Promise<void> {
        const hosts = await Registry.query();
        const links = new Array<Hole>();

        hosts.forEach((host) => {
            if (host.worlds.length > 0) {
                this.tryAddConnection(host);
            }

            host.worlds.forEach(world => {
                const connect = `${host.url}/${world.worldKey}`;
                this.worlds[connect] = world;
                if (!world.hook.Hidden && this.primaryServers.indexOf(host.url) > -1)
                {
                    var button = (connect != this.currentConnect)
                        ? html`<button class='playnow'>Play Now</button>`
                        : html`<button class='playnow'>ACTIVE</button>`;

                    var thumbnail = world.hook.thumbnail ?? 'img/worlds/openspace.png'
                    
                    links.push(html`
                        <a href="#" rel="nofollow,noindex" class="world" data-connect="${connect}">
                            <img src="${thumbnail}" />
                            <div class="explain">
                                <div class="title">${world.hook.name}</div>
                                <span>
                                    ${world.hook.description}
                                </span>
                            </div>
                            ${button}
                            <div class="worldstats">
                                <div class="ping"><span class="${host.url.replaceAll('.', '-') + "-ping"}">---</span></div>
                                <div class="players">players: ${world.advertisedPlayers}</div>
                            </div>
                        </a>
                    `);
                }
            });
        });

        render(
            document.getElementById('worlds')!,
            html`${links}`
        );

    }

    checkHosts(): void {
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

    updateHost(hostname: string, latency: number): void {
        var url:URL;
        try
        {
            url = new URL(hostname);
        }
        catch
        {
            url = new URL(`wss://${hostname}`);
        }

        document.querySelectorAll(`.${url.host.replaceAll('.', '-')}-ping`).forEach(e => {
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

    clearHosts(): void {
        for (let connect in this.connections)
            this.connections[connect]?.disconnect();

        this.connections = {};
    }

    show() {
        console.log('landing show');
        this.unhideUI();

        this.firstLoad = false;
        if (!this.firstLoad) {
            this.pingEnabled = true;
            document.getElementById('worlds')?.classList.add('pingenabled');
        }

        this.firstLoad = false;

        this.checkRegistry();
        this.checkHosts();
        window.document.body.classList.add('landing');
        this.setElementDisplay('gameArea', 'none');
        this.setElementDisplay('spawnscreen', 'none');
        this.setElementDisplay('lobby', '');
        this.visible = true;
    }

    hide() {
        window.document.body.classList.remove('landing');
        this.setElementDisplay('gameArea', '');
        this.setElementDisplay('spawnscreen', '');
        this.setElementDisplay('lobby', 'none');
        this.visible = false;
    }

    private setElementDisplay(id: string, styleDisplayValue: string) {
        const element = document.getElementById(id)
        if (element)
            element.style.display = styleDisplayValue;
    }

    private async launch(connect: string): Promise<void> {
        this.hide();

        this.clearHosts();

        if (this.currentConnect != connect) {

            var url = (new URL(document.location?.toString()));
            url?.searchParams.delete('world');
            window.history.pushState({}, '', url);
    
            this.currentConnect = connect;
            bus.emit("worldjoin", connect);
        }
    }

    private onArenasClick(e: Event) {
        this.show();
    }

    private onWorldClick(e: Event) {
        const world = (<HTMLElement>(e.target))?.closest('.world');
        const connect = world?.attributes.getNamedItem('data-connect')?.value;

        console.log('world click');
        e.preventDefault();

        if (connect)
            this.launch(connect);

    }

    private unhideUI()
    {
        if (this.entireUIHidden)
        {
            this.entireUIHidden = false;
            document.body.style.display = "";
        }
    }

    private querystringConfig(): boolean {
        try {
            var params = (new URL(document.location?.toString()))?.searchParams;
            if (params) {

                let world = params.get("world");
                if (world) {
                    this.launch(world);
                    this.unhideUI();
                    
                    return true;
                }
            }
        }
        catch (e) {
            console.log(`exception while parsing querystring: ${e}`);
        }
        return false;
    }

    async initialize(): Promise<void> {
        try {
            this.timer = window.setInterval(() => this.checkHosts(), 500);

            document.body.classList.add('dead');

            document.getElementById('worlds')?.addEventListener("click", (e) => this.onWorldClick(e));
            document.getElementById('arenas')?.addEventListener("click", (e) => this.onArenasClick(e));

            if (!this.querystringConfig())
                this.show();
        }
        catch (e) {
            console.log(e);
        }
    }
}

new Landing();