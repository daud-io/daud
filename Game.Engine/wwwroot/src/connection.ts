import { flatbuffers } from "flatbuffers";
import * as fb from "./game_generated";
import { Settings } from "./settings";
import { addSecretShips } from "./controls";
import { getToken } from "./discord";
import bus from "./bus";
import { Vector2 } from "@babylonjs/core";

export type LeaderboardEntry = { FleetID: number; Name: string; Color: string; Score: number; Position: Vector2; Token: boolean; ModeData: any };
export type LeaderboardType = {
    Type: string;
    Entries: LeaderboardEntry[];
    Record: {
        Name: string;
        Color: string;
        Score: number;
        Token: boolean;
    };
};
export class Connection {
    onView?: (view: fb.NetWorldView) => void;
    reloading = false;
    disconnecting = false;
    connected = false;
    framesPerSecond = 0;
    viewsPerSecond = 0;
    updatesPerSecond = 0;
    statBytesUp = 0;
    statBytesDown = 0;
    statBytesDownPerSecond = 0;
    statBytesUpPerSecond = 0;
    latency = 0;
    serverClockOffset = -1;
    simulateLatency = 0;
    socket?: WebSocket;
    pingSent?: number;
    bandwidthThrottle = Settings.bandwidth;
    autoReload = true;
    statPongCount = 0;
    hook: any;
    lastControlPacket: Uint8Array;

    constructor(onView?: (view: fb.NetWorldView) => void) {
        this.hook = null;
        this.onView = onView;

        setInterval(() => {
            this.statBytesDownPerSecond = this.statBytesDown;
            this.statBytesUpPerSecond = this.statBytesUp;

            this.statBytesUp = 0;
            this.statBytesDown = 0;
        }, 1000);

        this.lastControlPacket = new Uint8Array(0);
    }
    disconnect(): void {
        if (this.socket) {
            this.disconnecting = true;
            this.socket.close();
        }
    }
    connect(worldKey?: string): void {
        let url: string = window.location.protocol === "https:" ? "wss:" : "ws:";

        let hostname = "daud.io";

        if (worldKey) {
            const worldKeyParse = worldKey.match(/^(.*?)\/(.*)$/);
            if (worldKeyParse) {
                hostname = worldKeyParse[1];
                worldKey = worldKeyParse[2];
            }
        }

        url += `//${hostname}/api/v1/connect?`;

        if (worldKey) url += `world=${encodeURIComponent(worldKey)}&`;

        this.hook = null;
        this.serverClockOffset = -1;

        if (this.socket) {
            this.socket.onclose = null;
            this.socket.close();
        }

        this.socket = new WebSocket(url);
        this.socket.binaryType = "arraybuffer";

        this.socket.onmessage = (event) => {
            this.onMessage(event);
        };

        this.socket.onerror = () => {
            document.body.classList.add("connectionerror");
        };

        this.socket.onopen = () => {
            document.body.classList.remove("connectionerror");
            this.onOpen();
        };
        this.socket.onclose = (event) => {
            this.onClose(event);
        };
    }
    sendPing(): void {
        const builder = new flatbuffers.Builder(0);

        fb.NetPing.startNetPing(builder);
        this.pingSent = performance.now();

        fb.NetPing.addTime(builder, this.pingSent);
        fb.NetPing.addLatency(builder, this.latency);
        fb.NetPing.addVps(builder, this.viewsPerSecond);
        fb.NetPing.addUps(builder, this.updatesPerSecond);
        fb.NetPing.addFps(builder, this.framesPerSecond);
        fb.NetPing.addCs(builder, 0);
        fb.NetPing.addBackgrounded(builder, this.framesPerSecond < 1);
        fb.NetPing.addBandwidthThrottle(builder, this.bandwidthThrottle);

        const ping = fb.NetPing.endNetPing(builder);

        fb.NetQuantum.startNetQuantum(builder);
        fb.NetQuantum.addMessageType(builder, fb.AllMessages.NetPing);
        fb.NetQuantum.addMessage(builder, ping);
        const quantum = fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
    }

    sendExit(): void {
        const builder = new flatbuffers.Builder(0);

        fb.NetExit.startNetExit(builder);

        fb.NetExit.addCode(builder, 0);
        const exitmessage = fb.NetExit.endNetExit(builder);

        fb.NetQuantum.startNetQuantum(builder);
        fb.NetQuantum.addMessageType(builder, fb.AllMessages.NetExit);
        fb.NetQuantum.addMessage(builder, exitmessage);
        const quantum = fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
    }

    sendAuthenticate(token: string): void {
        const builder = new flatbuffers.Builder(0);

        const stringToken = builder.createString(token || "");

        fb.NetAuthenticate.startNetAuthenticate(builder);
        fb.NetAuthenticate.addToken(builder, stringToken);
        const auth = fb.NetAuthenticate.endNetAuthenticate(builder);

        fb.NetQuantum.startNetQuantum(builder);
        fb.NetQuantum.addMessageType(builder, fb.AllMessages.NetAuthenticate);
        fb.NetQuantum.addMessage(builder, auth);
        const quantum = fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
        console.log("sent auth");
    }

    sendSpawn(name?: string, color?: string, ship?: string, token?: string): void {
        const builder = new flatbuffers.Builder(0);

        const stringColor = builder.createString(color || "gray");
        const stringName = builder.createString(name || "unknown");
        const stringShip = builder.createString(ship || "ship_gray");
        const stringToken = builder.createString(token || "");

        fb.NetSpawn.startNetSpawn(builder);
        fb.NetSpawn.addColor(builder, stringColor);
        fb.NetSpawn.addName(builder, stringName);
        fb.NetSpawn.addShip(builder, stringShip);
        fb.NetSpawn.addToken(builder, stringToken);
        const spawn = fb.NetSpawn.endNetSpawn(builder);

        fb.NetQuantum.startNetQuantum(builder);
        fb.NetQuantum.addMessageType(builder, fb.AllMessages.NetSpawn);
        fb.NetQuantum.addMessage(builder, spawn);
        const quantum = fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
        console.log("spawned");
    }

    sendControl(
        boost: boolean, 
        shoot: boolean, 
        x: number, 
        y: number, 
        spectateControl: string, 
        customDataJson: string
    ): void {
        const builder = new flatbuffers.Builder(0);

        let spectateString: number | undefined = undefined;
        let customDataJsonString: number | undefined = undefined;

        if (spectateControl) spectateString = builder.createString(spectateControl);
        if (customDataJson) customDataJsonString = builder.createString(customDataJson);

        fb.NetControlInput.startNetControlInput(builder);
        fb.NetControlInput.addBoost(builder, boost);
        fb.NetControlInput.addShoot(builder, shoot);
        fb.NetControlInput.addX(builder, x);
        fb.NetControlInput.addY(builder, y);
        if (spectateString) fb.NetControlInput.addSpectateControl(builder, spectateString);
        if (customDataJsonString) fb.NetControlInput.addCustomData(builder, customDataJsonString);

        const input = fb.NetControlInput.endNetControlInput(builder);

        fb.NetQuantum.startNetQuantum(builder);
        fb.NetQuantum.addMessageType(builder, fb.AllMessages.NetControlInput);
        fb.NetQuantum.addMessage(builder, input);
        const quantum = fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        const newControlPacket = builder.asUint8Array();
        if (this.lastControlPacket.length != newControlPacket.length)
        {
            this.send(newControlPacket);
            this.lastControlPacket = newControlPacket;
        }
        else
            for(let i=0; i<newControlPacket.length; i++)
                if (newControlPacket[i] != this.lastControlPacket[i])
                {
                    this.send(newControlPacket);
                    this.lastControlPacket = newControlPacket;
                    break;
                }

    }

    send(databuffer: Uint8Array): void {
        if (this.socket && this.socket.readyState === 1) {
            this.socket.send(databuffer);
            this.statBytesUp += databuffer.length;
        }
    }

    onOpen(): void {
        this.connected = true;
        console.log("connected");
        this.sendPing();
        this.sendAuthenticate(getToken());

        if (this.reloading) {
            window.location.reload();
            this.reloading = false;
        }
    }

    onClose(event: CloseEvent): void {
        console.log("disconnected");
        this.connected = false;

        if (!this.disconnecting && this.autoReload) {
            if (event.reason != "Normal closure") {
                this.reloading = true;
            }

            this.connect();
        }
        this.disconnecting = false;
    }

    onMessage(event: MessageEvent): void {
        const data = new Uint8Array(event.data);
        const buf = new flatbuffers.ByteBuffer(data);

        this.statBytesDown += data.byteLength;

        const quantum = fb.NetQuantum.getRootAsNetQuantum(buf);

        const messageType = quantum.messageType();

        if (messageType == fb.AllMessages.NetWorldView) {
            const message = quantum.message(new fb.NetWorldView())!;

            if (this.onView) this.onView(message);
        }
        if (messageType == fb.AllMessages.NetPing) {
            const message = quantum.message(new fb.NetPing())!;
            if (this.pingSent) {
                
                this.statPongCount++;
                this.latency = performance.now() - this.pingSent;

                let offset = Settings.latencyOffset;
                if (Settings.latencyMode == "server")
                    offset += -this.latency/2;
                
                let newClockOffset = this.pingSent - message.time()  + offset;

                if (this.serverClockOffset == -1)
                    this.serverClockOffset = newClockOffset;

                this.serverClockOffset = 0.80 * this.serverClockOffset + 0.20 * newClockOffset;
                console.log("tweened clock offset:", this.serverClockOffset);

                setTimeout(() => {
                    if (this.connected) {
                        this.sendPing();
                    }
                }, 250);
            }
        }
        if (messageType == fb.AllMessages.NetEvent) {
            const message = quantum.message(new fb.NetEvent())!;

            const event = {
                type: message.type()!,
                data: JSON.parse(message.data()!),
            };

            if (event.type == "hook") {
                this.hook = event.data;
                bus.emit('hook', this.hook);
            }

            if (event.data.roles) addSecretShips(event.data.roles);
        }
        if (messageType == fb.AllMessages.NetLeaderboard) {
            const message = quantum.message(new fb.NetLeaderboard())!;

            const entriesLength = message.entriesLength();
            const entries: LeaderboardEntry[] = [];
            for (let i = 0; i < entriesLength; i++) {
                const entry = message.entries(i)!;
                entries.push({
                    FleetID: entry.fleetID(),
                    Name: entry.name()!,
                    Color: entry.color()!,
                    Score: entry.score(),
                    Position: new Vector2(entry.position()!.x(), entry.position()!.y()),
                    Token: entry.token(),
                    ModeData: JSON.parse(entry.modeData()!) || { flagStatus: "home" },
                });
            }

            const record = message.record();

            let recordModel = {
                Name: "",
                Color: "red",
                Score: 0,
                Token: false,
            };

            if (record) {
                recordModel = {
                    Name: record.name()!,
                    Color: record.color()!,
                    Score: record.score(),
                    Token: record.token(),
                };
            }
            bus.emit("leaderboard", {
                Type: message.type()!,
                Entries: entries,
                Record: recordModel,
            });
        }
    }
}
