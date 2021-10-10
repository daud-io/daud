import { Settings } from "./settings";
import { addSecretShips } from "./controls";
import { getToken } from "./discord";
import * as bus from "./bus";
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
import { AdvancedDynamicTexture } from "@babylonjs/gui";

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
    viewCPU = 0;
    statViewCPUPerSecond = 0;
    updatesPerSecond = 0;
    statBytesUp = 0;
    statBytesDown = 0;
    statBytesDownPerSecond = 0;
    statBytesUpPerSecond = 0;
    latency = 0;
    minimumLatency = -1;
    minimumLatencyNext = -1;
    minimumLatencyStart = -1;
    maximumLatencyNext = 0;
    minimumLatencyWindow = 30000;
    latencyWindowFirst = true;
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
    pingMode: boolean = false;
    connectionTime: number = 0;
    ripple: number = 0;
    earliestOffsetNext: number = -1;
    builder: Builder = new Builder(1024);
    cacheSize: number = 0;

    viewBuffer: Uint8Array;
    viewBufferSpace: number;
    viewBufferContents: { offset: number, length: number }[] = [];


    constructor() {
        setInterval(() => {
            this.statBytesDownPerSecond = this.statBytesDown;
            this.statBytesUpPerSecond = this.statBytesUp;
            this.statViewCPUPerSecond = this.viewCPU;

            this.statBytesUp = 0;
            this.statBytesDown = 0;
            this.viewCPU = 0;
        }, 1000);


        setInterval(() => {
            if (this.connected) {
                this.sendPing();
            }

            if (this.minimumLatencyStart < performance.now()) {
                console.log('new latency window: ' + this.minimumLatencyNext);
                this.minimumLatencyStart = performance.now() + this.minimumLatencyWindow;
                if (this.minimumLatencyNext != -1) {
                    this.minimumLatency = this.minimumLatencyNext;
                    this.ripple = Math.max(this.maximumLatencyNext - this.minimumLatencyNext, 0);
                    this.latencyWindowFirst = false;
                    this.earliestOffset = this.earliestOffsetNext;
                }

                this.earliestOffsetNext = -1;
                this.minimumLatencyNext = -1;
                this.maximumLatencyNext = 0;
            }
        }, 250);

        this.viewBuffer = new Uint8Array(5 * 1024 * 1024);
        this.viewBufferSpace = this.viewBuffer.length;
    }

    disconnect(): void {
        if (this.socket) {
            this.disconnecting = true;
            this.socket.close();
        }
    }
    connect(worldKey: string): void {
        const worldKeyParse = worldKey.match(/^(.*)\/(.*?)$/);
        if (!worldKeyParse)
        {
            throw "bad world key";
        }
        
        let hostname = worldKeyParse[1];
        if (hostname.indexOf('://') == -1)
            hostname = "wss://" + hostname;
        let worldName = worldKeyParse[2];
        let url = `${hostname}/api/v1/connect?world=${encodeURIComponent(worldName)}&`;

        this.hook = null;
        this.serverClockOffset = -1;
        this.minimumLatency = -1;
        this.earliestOffset = -1;
        this.earliestOffsetNext = -1;
        this.latencyWindowFirst = true;
        this.minimumLatencyStart = -1;
        this.minimumLatencyNext = -1;
        this.maximumLatencyNext = 0;


        if (this.socket) {
            this.socket.onclose = null;
            this.socket.onmessage = null;
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
        this.builder.clear();

        NetPing.startNetPing(this.builder);
        this.pingSent = performance.now();

        NetPing.addTime(this.builder, this.pingSent);
        NetPing.addClienttime(this.builder, this.pingSent);
        NetPing.addLatency(this.builder, this.latency);
        NetPing.addVps(this.builder, this.viewsPerSecond);
        NetPing.addUps(this.builder, this.updatesPerSecond);
        NetPing.addFps(this.builder, this.framesPerSecond);
        NetPing.addCs(this.builder, this.cacheSize);
        NetPing.addBackgrounded(this.builder, this.framesPerSecond < 1);
        NetPing.addBandwidththrottle(this.builder, this.bandwidthThrottle);

        const ping = NetPing.endNetPing(this.builder);

        NetQuantum.startNetQuantum(this.builder);
        NetQuantum.addMessageType(this.builder, AllMessages.NetPing);
        NetQuantum.addMessage(this.builder, ping);
        const quantum = NetQuantum.endNetQuantum(this.builder);

        this.builder.finish(quantum);

        this.send(this.builder.asUint8Array());
    }

    sendExit(): void {
        this.builder.clear();
        NetExit.startNetExit(this.builder);
        NetExit.addCode(this.builder, 0);
        const exitmessage = NetExit.endNetExit(this.builder);

        NetQuantum.startNetQuantum(this.builder);
        NetQuantum.addMessageType(this.builder, AllMessages.NetExit);
        NetQuantum.addMessage(this.builder, exitmessage);
        const quantum = NetQuantum.endNetQuantum(this.builder);

        this.builder.finish(quantum);

        this.send(this.builder.asUint8Array());
    }

    sendAuthenticate(token: string): void {
        if (token != '')
            return;

        this.builder.clear();

        const stringToken = this.builder.createString(token || "");

        NetAuthenticate.startNetAuthenticate(this.builder);
        NetAuthenticate.addToken(this.builder, stringToken);
        const auth = NetAuthenticate.endNetAuthenticate(this.builder);

        NetQuantum.startNetQuantum(this.builder);
        NetQuantum.addMessageType(this.builder, AllMessages.NetAuthenticate);
        NetQuantum.addMessage(this.builder, auth);
        const quantum = NetQuantum.endNetQuantum(this.builder);

        this.builder.finish(quantum);

        this.send(this.builder.asUint8Array());
        console.log("sent auth");
    }

    sendSpawn(name?: string, color?: string, ship?: string, token?: string): void {
        this.builder.clear();

        const stringColor = this.builder.createString(color || "gray");
        const stringName = this.builder.createString(name || "");
        const stringShip = this.builder.createString(ship || "ship_gray");
        const stringToken = this.builder.createString(token || "");

        NetSpawn.startNetSpawn(this.builder);
        NetSpawn.addColor(this.builder, stringColor);
        NetSpawn.addName(this.builder, stringName);
        NetSpawn.addShip(this.builder, stringShip);
        NetSpawn.addToken(this.builder, stringToken);
        const spawn = NetSpawn.endNetSpawn(this.builder);

        NetQuantum.startNetQuantum(this.builder);
        NetQuantum.addMessageType(this.builder, AllMessages.NetSpawn);
        NetQuantum.addMessage(this.builder, spawn);
        const quantum = NetQuantum.endNetQuantum(this.builder);

        this.builder.finish(quantum);

        this.send(this.builder.asUint8Array());
        console.log("spawned");
        bus.emit("spawn", name ?? "", ship ?? "");
    }

    sendControl(boost: boolean, shoot: boolean, x: number, y: number, spectateControl: string, customDataJson: string): void {
        this.builder.clear();

        let spectateString: number | undefined = undefined;
        let customDataJsonString: number | undefined = undefined;

        if (spectateControl) spectateString = this.builder.createString(spectateControl);
        if (customDataJson) customDataJsonString = this.builder.createString(customDataJson);

        NetControlInput.startNetControlInput(this.builder);
        NetControlInput.addBoost(this.builder, boost);
        NetControlInput.addShoot(this.builder, shoot);
        NetControlInput.addX(this.builder, x);
        NetControlInput.addY(this.builder, y);
        if (spectateString) NetControlInput.addSpectatecontrol(this.builder, spectateString);
        if (customDataJsonString) NetControlInput.addCustomdata(this.builder, customDataJsonString);

        const input = NetControlInput.endNetControlInput(this.builder);

        NetQuantum.startNetQuantum(this.builder);
        NetQuantum.addMessageType(this.builder, AllMessages.NetControlInput);
        NetQuantum.addMessage(this.builder, input);
        const quantum = NetQuantum.endNetQuantum(this.builder);

        this.builder.finish(quantum);
        this.send(this.builder.asUint8Array());
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
        this.connectionTime = performance.now();
        bus.emit("connected", this);
        this.sendPing();

        if (!this.pingMode) {
            this.sendAuthenticate(getToken());

            if (this.reloading) {
                window.location.reload();
                this.reloading = false;
            }
        }
    }

    onClose(event: CloseEvent): void {
        this.connected = false;

        if (!this.disconnecting && this.autoReload) {
            if (event.reason != "Normal closure") {
                this.reloading = true;
            }
            //console.log('reconnecting');

            //this.connect();
        }
        this.disconnecting = false;
    }

    handleNetWorldViewBuffer(newView: Uint8Array): void {
        while (this.viewBufferSpace < newView.length) {
            const addedCapacity = this.viewBuffer.length;
            this.viewBufferSpace += addedCapacity;
            let newBuffer = new Uint8Array(this.viewBuffer.length + addedCapacity);
            newBuffer.set(this.viewBuffer, 0);
            this.viewBuffer = newBuffer;
        }

        var contentsHeader = {
            offset: this.viewBuffer.length - this.viewBufferSpace,
            length: newView.length
        };

        this.viewBufferContents.push(contentsHeader);
        this.viewBuffer.set(newView, contentsHeader.offset);
        this.viewBufferSpace -= contentsHeader.length;
    }

    dispatchWorldViews() {
        for (let i in this.viewBufferContents) {
            const header = this.viewBufferContents[i];
            const data = this.viewBuffer.subarray(header.offset, header.offset + header.length);
            const buf = new ByteBuffer(data);
            const quantum = NetQuantum.getRootAsNetQuantum(buf);
            this.handleNetWorldView(quantum.message(new NetWorldView()));
        }
        this.viewBufferContents.length = 0;
        this.viewBufferSpace = this.viewBuffer.length;
    }


    handleNetWorldView(view: NetWorldView): void {
        const offset = performance.now() - view.time();

        if (offset < this.earliestOffsetNext || this.earliestOffsetNext == -1) {
            this.earliestOffsetNext = offset;

            if (this.latencyWindowFirst)
                this.earliestOffset = offset;
        }

        this.serverClockOffset = this.earliestOffset;

        const worldviewStart = performance.now();
        bus.emit("worldview", view);
        this.viewCPU += performance.now() - worldviewStart;

    }

    handleNetPing(message: NetPing): void {
        if (message.clienttime() < this.connectionTime)
            return;

        this.statPongCount++;

        this.latency = performance.now() - message.clienttime();
        if (this.latency < this.minimumLatencyNext || this.minimumLatencyNext == -1) {
            this.minimumLatencyNext = this.latency;
            if (this.latencyWindowFirst)
                this.minimumLatency = this.minimumLatencyNext;
        }
        if (this.latency > this.maximumLatencyNext)
            this.maximumLatencyNext = this.latency;
    }

    handleNetEvent(message: NetEvent): void {
        const eventObject = {
            type: message.type()!,
            data: JSON.parse(message.data()!),
        };

        if (eventObject.type == "hook") {
            this.hook = eventObject.data;
            bus.emit("hook", this.hook);
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
                //this.handleNetWorldViewBuffer(data);
                this.handleNetWorldView(quantum.message(new NetWorldView()));
                break;
            case AllMessages.NetPing:
                this.handleNetPing(quantum.message(new NetPing()));
                break;
            case AllMessages.NetEvent:
                this.handleNetEvent(quantum.message(new NetEvent()));
                break;
            case AllMessages.NetLeaderboard:
                this.handleNetLeaderboard(quantum.message(new NetLeaderboard()));
                break;
        }
    }
}
