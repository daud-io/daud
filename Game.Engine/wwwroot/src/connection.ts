import { flatbuffers } from "./flatbuffers";
import * as fb from "./game_generated";
import { Cache } from "./cache";
import { Settings } from "./settings";
import { Vector2 } from "./Vector2";
import { Controls } from "./controls";

export class Connection {
    onView: (view: fb.NetWorldView) => void;
    onLeaderboard: (leaderboard: { Type: any; Entries: any; Record: any }) => void;
    onConnected: () => void;
    reloading: boolean;
    disconnecting: boolean;
    connected: boolean;
    framesPerSecond: number;
    viewsPerSecond: number;
    updatesPerSecond: number;
    statBytesUp: number;
    statBytesDown: number;
    statBytesDownPerSecond: number;
    statBytesUpPerSecond: number;
    isBackgrounded: boolean;
    fb: any;
    latency: number;
    minLatency: number;
    simulateLatency: number;
    socket: WebSocket;
    pingSent: number;
    bandwidthThrottle: number;
    autoReload: boolean;
    statPongCount: number;
    connectionStatusReporting: boolean;
    hook: any;

    constructor() {
        this.onView = (view) => {};
        this.onLeaderboard = (leaderboard) => {};
        this.onConnected = () => {};
        this.reloading = false;
        this.disconnecting = false;
        this.connected = false;
        this.autoReload = true;
        this.connectionStatusReporting = true;

        this.statBytesUp = 0;
        this.statBytesDown = 0;
        this.statBytesDownPerSecond = 0;
        this.statBytesUpPerSecond = 0;
        this.statPongCount = 0;

        this.hook = null;

        this.latency = 0;
        this.minLatency = 999;
        this.simulateLatency = 0;

        this.bandwidthThrottle = Settings.bandwidth;

        setInterval(() => {
            if (this.connected) {
                this.sendPing();
            }
        }, 250);

        setInterval(() => {
            this.statBytesDownPerSecond = this.statBytesDown;
            this.statBytesUpPerSecond = this.statBytesUp;

            this.statBytesUp = 0;
            this.statBytesDown = 0;
        }, 1000);
    }
    disconnect() {
        if (this.socket) {
            this.disconnecting = true;
            this.socket.close();
        }
    }
    connect(worldKey?: string) {
        let url: string;

        if (window.location.protocol === "https:") {
            url = "wss:";
        } else {
            url = "ws:";
        }

        let hostname = window.location.host;

        if (!hostname) {
            hostname = "daud.io";
        }

        if (worldKey) {
            const worldKeyParse = worldKey.match(/^(.*?)\/(.*)$/);
            if (worldKeyParse) {
                hostname = worldKeyParse[1];
                worldKey = worldKeyParse[2];
            }
        }

        url += `//${hostname}`;
        url += "/api/v1/connect?";

        if (worldKey) url += `world=${encodeURIComponent(worldKey)}&`;

        this.hook = null;

        if (this.socket) {
            this.socket.onclose = () => {};
            this.socket.close();
        }

        this.socket = new WebSocket(url);
        this.socket.binaryType = "arraybuffer";

        this.socket.onmessage = (event) => {
            if (this.simulateLatency > 0) {
                setTimeout(() => {
                    this.onMessage(event);
                }, this.simulateLatency);
            } else this.onMessage(event);
        };

        this.socket.onerror = (error) => {
            if (this.connectionStatusReporting) document.body.classList.add("connectionerror");
        };

        this.socket.onopen = (event) => {
            if (this.connectionStatusReporting) document.body.classList.remove("connectionerror");
            this.onOpen(event);
        };
        this.socket.onclose = (event) => {
            this.onClose(event);
        };
    }
    sendPing() {
        const builder = new (flatbuffers as any).Builder(0);

        fb.NetPing.startNetPing(builder);
        this.pingSent = performance.now();

        //fb.Ping.addTime(builder, this.pingSent);
        fb.NetPing.addLatency(builder, this.latency);
        fb.NetPing.addVps(builder, this.viewsPerSecond);
        fb.NetPing.addUps(builder, this.updatesPerSecond);
        fb.NetPing.addFps(builder, this.framesPerSecond);
        fb.NetPing.addCs(builder, Cache.count);
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

    sendExit() {
        const builder = new (flatbuffers as any).Builder(0);

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

    sendAuthenticate(token) {
        const builder = new (flatbuffers as any).Builder(0);

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

    sendSpawn(name, color, ship, token) {
        const builder = new (flatbuffers as any).Builder(0);

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

    sendControl(angle, boost, shoot, x, y, spectateControl, customDataJson) {
        const builder = new (flatbuffers as any).Builder(0);

        let spectateString = false;
        let customDataJsonString = false;

        if (spectateControl) spectateString = builder.createString(spectateControl);
        if (customDataJson) customDataJsonString = builder.createString(customDataJson);

        fb.NetControlInput.startNetControlInput(builder);
        fb.NetControlInput.addAngle(builder, angle);
        fb.NetControlInput.addBoost(builder, boost);
        fb.NetControlInput.addShoot(builder, shoot);
        fb.NetControlInput.addX(builder, x);
        fb.NetControlInput.addY(builder, y);
        if (spectateControl) fb.NetControlInput.addSpectateControl(builder, spectateString);
        if (customDataJson) fb.NetControlInput.addCustomData(builder, customDataJsonString);

        const input = fb.NetControlInput.endNetControlInput(builder);

        fb.NetQuantum.startNetQuantum(builder);
        fb.NetQuantum.addMessageType(builder, fb.AllMessages.NetControlInput);
        fb.NetQuantum.addMessage(builder, input);
        const quantum = fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
    }

    send(databuffer) {
        if (this.socket && this.socket.readyState === 1) {
            const self = this;
            if (this.simulateLatency > 0) {
                setTimeout(() => {
                    self.socket.send(databuffer);
                }, this.simulateLatency);
            } else this.socket.send(databuffer);

            this.statBytesUp += databuffer.length;
        }
    }

    onOpen(event) {
        this.connected = true;
        console.log("connected");
        this.sendPing();
        this.onConnected();

        if (this.reloading) {
            window.location.reload();
            this.reloading = false;
        }
    }

    onClose(event) {
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

    onMessage(event) {
        const data = new Uint8Array(event.data);
        const buf = new (flatbuffers as any).ByteBuffer(data);

        this.statBytesDown += data.byteLength;

        const quantum = fb.NetQuantum.getRootAsNetQuantum(buf);

        const messageType = quantum.messageType();

        if (messageType == fb.AllMessages.NetWorldView) {
            let message = quantum.message(new fb.NetWorldView());

            this.onView(message);
        }
        if (messageType == fb.AllMessages.NetPing) {
            if (this.pingSent) {
                this.statPongCount++;
                this.latency = performance.now() - this.pingSent;
                if (this.latency > 0 && this.latency < this.minLatency) this.minLatency = this.latency;
            }
        }
        if (messageType == fb.AllMessages.NetEvent) {
            let message = quantum.message(new fb.NetEvent());

            const event = {
                type: message.type(),
                data: JSON.parse(message.data()),
            };

            if (event.type == "hook") {
                this.hook = event.data;
            }

            if (event.data.roles !== undefined) {
                window.discordData = event;
            }
            Controls.addSecretShips(event);
        }
        if (messageType == fb.AllMessages.NetLeaderboard) {
            let message = quantum.message(new fb.NetLeaderboard());

            const entriesLength = message.entriesLength();
            const entries = [];
            for (let i = 0; i < entriesLength; i++) {
                const entry = message.entries(i);

                entries.push({
                    FleetID: entry.fleetID(),
                    Name: entry.name(),
                    Color: entry.color(),
                    Score: entry.score(),
                    Position: new Vector2(entry.position().x(), entry.position().y()),
                    Token: entry.token(),
                    ModeData: JSON.parse(entry.modeData()) || { flagStatus: "home" },
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
                    Name: record.name(),
                    Color: record.color(),
                    Score: record.score(),
                    Token: record.token(),
                };
            }
            this.onLeaderboard({
                Type: message.type(),
                Entries: entries,
                Record: recordModel,
            });
        }
    }
}
