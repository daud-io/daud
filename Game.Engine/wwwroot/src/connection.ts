
import { Settings } from "./settings";
import { addSecretShips } from "./controls";
import { getToken } from "./discord";
import bus from "./bus";
import { Vector2 } from "@babylonjs/core";
import { NetWorldView } from "./daud-net/net-world-view";
import { NetPing } from "./daud-net/net-ping";
import { NetQuantum } from "./daud-net/net-quantum";
import { Builder, ByteBuffer } from "flatbuffers";
import { NetExit } from "./daud-net/net-exit";
import { AllMessages } from "./daud-net/all-messages";
import { NetAuthenticate } from "./daud-net/net-authenticate";
import { NetSpawn } from "./daud-net/net-spawn";
import { NetControlInput } from "./daud-net/net-control-input";
import { NetEvent } from "./daud-net/net-event";
import { NetLeaderboard } from "./daud-net/net-leaderboard";

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
    minimumLatency = -1;
    serverClockOffset = -1;
    simulateLatency = 0;
    socket?: WebSocket;
    pingSent?: number;
    bandwidthThrottle = Settings.bandwidth;
    autoReload = true;
    statPongCount = 0;
    hook: any = null;
    lastControlPacket: Uint8Array = new Uint8Array(0);
    earliestOffset: number = -1;

    constructor() {
        setInterval(() => {
            this.statBytesDownPerSecond = this.statBytesDown;
            this.statBytesUpPerSecond = this.statBytesUp;

            this.statBytesUp = 0;
            this.statBytesDown = 0;
        }, 1000);
    }

    disconnect(): void {
        if (this.socket) {
            this.disconnecting = true;
            this.socket.close();
        }
    }
    connect(worldKey?: string): void {
        console.log("connecting to " + worldKey);
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
        this.minimumLatency = -1;
        this.earliestOffset = -1;

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
        const builder = new Builder(0);

        NetPing.startNetPing(builder);
        this.pingSent = performance.now();

        NetPing.addTime(builder, this.pingSent);
        NetPing.addClienttime(builder, this.pingSent);
        NetPing.addLatency(builder, this.latency);
        NetPing.addVps(builder, this.viewsPerSecond);
        NetPing.addUps(builder, this.updatesPerSecond);
        NetPing.addFps(builder, this.framesPerSecond);
        NetPing.addCs(builder, 0);
        NetPing.addBackgrounded(builder, this.framesPerSecond < 1);
        NetPing.addBandwidththrottle(builder, this.bandwidthThrottle);

        const ping = NetPing.endNetPing(builder);

        NetQuantum.startNetQuantum(builder);
        NetQuantum.addMessageType(builder, AllMessages.NetPing);
        NetQuantum.addMessage(builder, ping);
        const quantum = NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
    }

    sendExit(): void {
        const builder = new Builder(0);

        NetExit.startNetExit(builder);

        NetExit.addCode(builder, 0);
        const exitmessage = NetExit.endNetExit(builder);

        NetQuantum.startNetQuantum(builder);
        NetQuantum.addMessageType(builder, AllMessages.NetExit);
        NetQuantum.addMessage(builder, exitmessage);
        const quantum = NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
    }

    sendAuthenticate(token: string): void {
        const builder = new Builder(0);

        const stringToken = builder.createString(token || "");

        NetAuthenticate.startNetAuthenticate(builder);
        NetAuthenticate.addToken(builder, stringToken);
        const auth = NetAuthenticate.endNetAuthenticate(builder);

        NetQuantum.startNetQuantum(builder);
        NetQuantum.addMessageType(builder, AllMessages.NetAuthenticate);
        NetQuantum.addMessage(builder, auth);
        const quantum = NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
        console.log("sent auth");
    }

    sendSpawn(name?: string, color?: string, ship?: string, token?: string): void {
        const builder = new Builder(0);

        const stringColor = builder.createString(color || "gray");
        const stringName = builder.createString(name || "unknown");
        const stringShip = builder.createString(ship || "ship_gray");
        const stringToken = builder.createString(token || "");

        NetSpawn.startNetSpawn(builder);
        NetSpawn.addColor(builder, stringColor);
        NetSpawn.addName(builder, stringName);
        NetSpawn.addShip(builder, stringShip);
        NetSpawn.addToken(builder, stringToken);
        const spawn = NetSpawn.endNetSpawn(builder);

        NetQuantum.startNetQuantum(builder);
        NetQuantum.addMessageType(builder, AllMessages.NetSpawn);
        NetQuantum.addMessage(builder, spawn);
        const quantum = NetQuantum.endNetQuantum(builder);

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
        const builder = new Builder(0);

        let spectateString: number | undefined = undefined;
        let customDataJsonString: number | undefined = undefined;

        if (spectateControl) spectateString = builder.createString(spectateControl);
        if (customDataJson) customDataJsonString = builder.createString(customDataJson);

        NetControlInput.startNetControlInput(builder);
        NetControlInput.addBoost(builder, boost);
        NetControlInput.addShoot(builder, shoot);
        NetControlInput.addX(builder, x);
        NetControlInput.addY(builder, y);
        if (spectateString) NetControlInput.addSpectatecontrol(builder, spectateString);
        if (customDataJsonString) NetControlInput.addCustomdata(builder, customDataJsonString);

        const input = NetControlInput.endNetControlInput(builder);

        NetQuantum.startNetQuantum(builder);
        NetQuantum.addMessageType(builder, AllMessages.NetControlInput);
        NetQuantum.addMessage(builder, input);
        const quantum = NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        const newControlPacket = builder.asUint8Array();
        if (this.lastControlPacket.length != newControlPacket.length) {
            this.send(newControlPacket);
            this.lastControlPacket = newControlPacket;
        }
        else
            for (let i = 0; i < newControlPacket.length; i++)
                if (newControlPacket[i] != this.lastControlPacket[i]) {
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

    currentWorldTime(): number {
        return performance.now() - this.serverClockOffset;
    }

    handleNetWorldView(view: NetWorldView): void {
        const offset = performance.now() - view.time();

        if (this.earliestOffset == -1)
            this.earliestOffset = offset;
        else
            this.earliestOffset = Math.min(this.earliestOffset, offset);

        bus.emit("worldview", view);
    }

    handleNetPing(message: NetPing): void {
        this.statPongCount++;

        this.latency = performance.now() - message.clienttime();
        if (this.latency < this.minimumLatency || this.minimumLatency == -1) {
            this.minimumLatency = this.latency;
        }
        
        this.serverClockOffset = this.earliestOffset;

        setTimeout(() => {
            if (this.connected) {
                this.sendPing();
            }
        }, 250);
    }

    handleNetEvent(message: NetEvent): void {
        const eventObject = {
            type: message.type()!,
            data: JSON.parse(message.data()!),
        };

        if (eventObject.type == "hook") {
            this.hook = eventObject.data;
            bus.emit('hook', this.hook);
        }

        if (eventObject.data.roles) addSecretShips(eventObject.data.roles);
    }

    handleNetLeaderboard(message: NetLeaderboard): void {
        const entriesLength = message.entriesLength();
        const entries: LeaderboardEntry[] = [];
        for (let i = 0; i < entriesLength; i++) {
            const entry = message.entries(i)!;
            entries.push({
                FleetID: entry.fleetid(),
                Name: entry.name()!,
                Color: entry.color()!,
                Score: entry.score(),
                Position: new Vector2(entry.position()!.x(), entry.position()!.y()),
                Token: entry.token(),
                ModeData: JSON.parse(entry.modedata()!) || { flagStatus: "home" },
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

    onMessage(event: MessageEvent): void {
        const data = new Uint8Array(event.data);
        const buf = new ByteBuffer(data);
        const quantum = NetQuantum.getRootAsNetQuantum(buf);
        const messageType = quantum.messageType();

        this.statBytesDown += data.byteLength;

        switch (messageType) {
            case AllMessages.NetWorldView:
                this.handleNetWorldView(quantum.message(new NetWorldView()))
                break;
            case AllMessages.NetPing:
                this.handleNetPing(quantum.message(new NetPing()))
                break;
            case AllMessages.NetEvent:
                this.handleNetEvent(quantum.message(new NetEvent()))
                break;
            case AllMessages.NetLeaderboard:
                this.handleNetLeaderboard(quantum.message(new NetLeaderboard()))
                break;

        }
    }
}
