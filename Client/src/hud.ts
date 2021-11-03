import { GameContainer } from "./gameContainer";
import * as bus from "./bus";

export class HUD {
    container: GameContainer;
    hudEL: HTMLElement;
    netwarnEL: HTMLElement;
    cpuwarnEL: HTMLElement;

    cpuVisible: boolean = false;
    netVisible: boolean = false;

    playerCount: number = 0;
    spectatorCount: number = 0;

    originalTitle: string;

    constructor(container: GameContainer) {
        this.container = container;
        this.originalTitle = window.document.title;
        this.hudEL = document.getElementById("hud")!;
        this.netwarnEL = document.getElementById("netwarn")!;
        this.cpuwarnEL = document.getElementById("cpuwarn")!;

        bus.on('worldview', (view) => {
            this.playerCount = view.playercount();
            this.spectatorCount = view.spectatorcount();
        });

        setInterval(() => this.update(), 1000);
    }

    update(): void {
        window.document.title = this.playerCount > 0 ? `DAUD | (${this.playerCount})` : this.originalTitle;
        var con = this.container.connection;
        let text = `\
            fps: ${con.framesPerSecond || 0} - \
            players: ${this.playerCount || 0} - \
            spectators: ${this.spectatorCount || 0} - \
            cpu: ${Math.floor(con.viewCPU) || 0} - \
            ping: ${Math.floor(con.latency) || 0} \
        `;

        let cpuwarn = false;
        let netwarn = false;

        if (con)
        {
            cpuwarn = con.viewCPU > 200;

            netwarn = 
               (con.latency > 200)
            || (con.ripple > 50)
            || ((con.socket?.bufferedAmount ?? 0) > 1024);
        }

        if (cpuwarn && !this.cpuVisible)
            this.cpuwarnEL.classList.add('active');
        if (!cpuwarn && this.cpuVisible)
            this.cpuwarnEL.classList.remove('active');
        this.cpuVisible = cpuwarn;

        if (netwarn && !this.netVisible)
            this.netwarnEL.classList.add('active');
        if (!cpuwarn && this.netVisible)
            this.netwarnEL.classList.remove('active');
        this.netVisible = netwarn;


        this.hudEL.innerText = text;
    }
}


