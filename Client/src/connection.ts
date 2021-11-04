import { Settings } from "./settings";
import { addSecretShips, Controls } from "./controls";
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
    autoReload = true;
    statPongCount = 0;
    hook: any = null;
    lastControlPacket: Uint8Array = new Uint8Array(0);
    earliestOffset: number = -1;
    connectionTime: number = 0;
    ripple: number = 0;
    earliestOffsetNext: number = -1;
    builder: Builder = new Builder(1024);
    cacheSize: number = 0;

    viewBuffer: ByteBuffer[] = [];

    messageBuffers = {
        Quantum: new NetQuantum(),
        WorldView: new NetWorldView(),
        Ping: new NetPing(),
        Event: new NetEvent(),
        Leaderboard: new NetLeaderboard()
    };


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

        bus.on('hook', (hook) => this.hook = hook);
    }

    disconnect(): void {
        if (this.connected)
        {
            this.connected = false;
            bus.emit("disconnected");
        }

        if (this.socket) {
            this.socket.close();
        }
    }

    resetTimingWindow() {
        console.log('reset timing window');
        this.serverClockOffset = -1;
        this.minimumLatency = -1;
        this.earliestOffset = -1;
        this.earliestOffsetNext = -1;
        this.latencyWindowFirst = true;
        this.minimumLatencyStart = -1;
        this.minimumLatencyNext = -1;
        this.maximumLatencyNext = 0;
    }

    newTimingWindow() {
        console.log('new timing window');
        this.minimumLatencyNext = -1;
        this.minimumLatencyStart = -1;
    }

    connect(worldKey: string): void {
        try
        {

            var url:URL;

            try
            {
                url = new URL(worldKey);
            }
            catch
            {
                url = new URL(`wss://${worldKey}`);
            }
            let apiURL = `${url.protocol}//${url.host}/api/v1/connect?world=${encodeURIComponent(url.pathname?.substr(1))}`;

            if (this.socket) {
                this.onClose();
                this.socket.onclose = null;
                this.socket.onmessage = null;
                this.socket.close();
            }

            this.socket = new WebSocket(apiURL);
            this.socket.binaryType = "arraybuffer";

            this.hook = null;

            this.resetTimingWindow();

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
            this.socket.onclose = () => {
                this.onClose();
            };
        }
        catch(e)
        {
            console.log('bad connection string: ' + worldKey);
        }

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
        NetPing.addBandwidththrottle(this.builder, Settings.bandwidth);

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

    sendControl(boost: boolean, shoot: boolean, x: number, y: number, spectateControl: string|undefined): void {
        this.builder.clear();

        let spectateString: number | undefined = undefined;

        if (spectateControl)
            spectateString = this.builder.createString(spectateControl);

        NetControlInput.startNetControlInput(this.builder);
        NetControlInput.addBoost(this.builder, boost);
        NetControlInput.addShoot(this.builder, shoot);
        NetControlInput.addX(this.builder, x);
        NetControlInput.addY(this.builder, y);
        
        if (spectateString) NetControlInput.addSpectatecontrol(this.builder, spectateString);

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
        this.sendAuthenticate(getToken());
    }

    onClose(): void {
            
        this.connected = false;
    }

    handleNetWorldViewBuffer(newView: ByteBuffer): void {
        this.viewBuffer.push(newView);
        // if we are backgrounds, fps=0, we can't buffer forever
        if (this.viewBuffer.length > 200)
            this.dispatchWorldViews();
    }

    dispatchWorldViews() {
        try
        {
            let start = performance.now();
            for (let i=0; i<this.viewBuffer.length; i++) {
                const buf = this.viewBuffer[i];
                const quantum = NetQuantum.getRootAsNetQuantum(buf);
                this.handleNetWorldView(quantum.message(new NetWorldView()));
            }
            
            //console.log(`dispatched ${this.viewBuffer.length} in ${performance.now()-start}`);
            this.viewBuffer.length = 0;
        }
        catch (e)
        {
            console.log('exception in dispatchWorlds');
        }

    }

    handleNetWorldView(view: NetWorldView, arrivalTime: number = -1): void {

        if (arrivalTime==-1) arrivalTime = performance.now();
        const offset = arrivalTime - view.time();

        if (offset < this.earliestOffsetNext || this.earliestOffsetNext == -1) {
            this.earliestOffsetNext = offset;

            if (this.latencyWindowFirst)
                this.earliestOffset = offset;
        }

        this.serverClockOffset = this.earliestOffset;

        const worldviewStart = performance.now();
        //bus.emit("worldview", view);
        bus.emitWorldview(view);
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

        var byteArray = new Uint8Array(event.data);
        var buffer = new ByteBuffer(byteArray);
        const quantum = NetQuantum.getRootAsNetQuantum(buffer, this.messageBuffers.Quantum);
        const messageType = quantum.messageType();
        this.statBytesDown += byteArray.length;

        switch (messageType) {
            case AllMessages.NetWorldView:
                this.handleNetWorldViewBuffer(buffer);
                //this.handleNetWorldView(quantum.message(this.messageBuffers.WorldView));
                break;
            case AllMessages.NetPing:
                this.handleNetPing(quantum.message(this.messageBuffers.Ping));
                break;
            case AllMessages.NetEvent:
                this.handleNetEvent(quantum.message(this.messageBuffers.Event));
                break;
            case AllMessages.NetLeaderboard:
                this.handleNetLeaderboard(quantum.message(this.messageBuffers.Leaderboard));
                break;
        }
    }
}
