import { flatbuffers } from "flatbuffers";
import { Game } from "./game_generated";
import { Cache } from "./cache";
import { Settings } from "./settings";
import { Vector2 } from "./Vector2";
import { Controls } from "./controls":

export class Connection {
    onView: (view: any) => void;
    onLeaderboard: (leaderboard: any) => void;
    onConnected: () => void;
    reloading: boolean;
    connected: boolean;
    framesPerSecond: number;
    viewsPerSecond: number;
    updatesPerSecond: number;
    statBytesUp: number;
    statBytesDown: number;
    statBytesDownPerSecond: number;
    statBytesUpPerSecond: number;
    fb: any;
    latency: number;
    minLatency: number;
    simulateLatency: number;
    socket: WebSocket;
    pingSent: number;
    constructor() {
        this.onView = view => {};
        this.onLeaderboard = leaderboard => {};
        this.onConnected = () => {};
        this.reloading = false;
        this.connected = false;

        this.statBytesUp = 0;
        this.statBytesDown = 0;
        this.statBytesDownPerSecond = 0;
        this.statBytesUpPerSecond = 0;

        const self = this;
        this.fb = Game.Engine.Networking.FlatBuffers;
        this.latency = 0;
        this.minLatency = 999;
        this.simulateLatency = 0;

        setInterval(() => {
            if (self.connected) {
                self.sendPing();
            }

            self.statBytesDownPerSecond = self.statBytesDown;
            self.statBytesUpPerSecond = self.statBytesUp;

            self.statBytesUp = 0;
            self.statBytesDown = 0;
        }, 1000);
    }
    disconnect() {
        if (this.socket) this.socket.close();
    }
    connect(world?) {
        let url;
        if (window.location.protocol === "https:") {
            url = "wss:";
        } else {
            url = "ws:";
        }

        let hostname = window.location.host;

        if (!hostname) {
            hostname = "daud.io";
        }

        if (world) {
            var worldKeyParse = world.match(/^(.*?)\/(.*)$/);
            if (worldKeyParse) {
                hostname = worldKeyParse[1];
                world = worldKeyParse[2];
            }
        }

        url += `//${hostname}`;
        url += "/api/v1/connect?";

        if (world) url += `world=${encodeURIComponent(world)}&`;

        if (this.socket) {
            this.socket.onclose = () => {};
            this.socket.close();
        }

        this.socket = new WebSocket(url);
        this.socket.binaryType = "arraybuffer";

        const self = this;

        this.socket.onmessage = event => {
            if (self.simulateLatency > 0) {
                setTimeout(() => {
                    self.onMessage(event);
                }, self.simulateLatency);
            } else self.onMessage(event);
        };

        this.socket.onerror = error => {
            document.body.classList.add("connectionerror");
        };

        this.socket.onopen = event => {
            document.body.classList.remove("connectionerror");
            self.onOpen(event);
        };
        this.socket.onclose = event => {
            self.onClose(event);
        };
    }
    sendPing() {
        const builder = new (<any>flatbuffers).Builder(0);

        this.fb.NetPing.startNetPing(builder);

        this.pingSent = performance.now();

        //this.fb.Ping.addTime(builder, this.pingSent);
        this.fb.NetPing.addLatency(builder, this.latency);
        this.fb.NetPing.addVps(builder, this.viewsPerSecond);
        this.fb.NetPing.addUps(builder, this.updatesPerSecond);
        this.fb.NetPing.addFps(builder, this.framesPerSecond);
        this.fb.NetPing.addCs(builder, Cache.count);
        this.fb.NetPing.addBackgrounded(builder, this.isBackgrounded);
        this.fb.NetPing.addBandwidthThrottle(builder, Settings.bandwidth);

        const ping = this.fb.NetPing.endNetPing(builder);

        this.fb.NetQuantum.startNetQuantum(builder);
        this.fb.NetQuantum.addMessageType(builder, this.fb.AllMessages.NetPing);
        this.fb.NetQuantum.addMessage(builder, ping);
        const quantum = this.fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
    }
    isBackgrounded(builder: any, isBackgrounded: any): any {
        throw new Error("Method not implemented.");
    }

    sendExit() {
        const builder = new (<any>flatbuffers).Builder(0);

        this.fb.NetExit.startNetExit(builder);

        this.fb.NetExit.addCode(builder, 0);
        const exitmessage = this.fb.NetExit.endNetExit(builder);

        this.fb.NetQuantum.startNetQuantum(builder);
        this.fb.NetQuantum.addMessageType(builder, this.fb.AllMessages.NetExit);
        this.fb.NetQuantum.addMessage(builder, exitmessage);
        const quantum = this.fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
    }

    sendAuthenticate(token) {
        const builder = new (<any>flatbuffers).Builder(0);

        const stringToken = builder.createString(token || "");

        this.fb.NetAuthenticate.startNetAuthenticate(builder);
        this.fb.NetAuthenticate.addToken(builder, stringToken);
        const auth = this.fb.NetAuthenticate.endNetAuthenticate(builder);

        this.fb.NetQuantum.startNetQuantum(builder);
        this.fb.NetQuantum.addMessageType(builder, this.fb.AllMessages.NetAuthenticate);
        this.fb.NetQuantum.addMessage(builder, auth);
        const quantum = this.fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
        console.log("sent auth");
    }

    sendSpawn(name, color, ship, token) {
        const builder = new (<any>flatbuffers).Builder(0);

        const stringColor = builder.createString(color || "gray");
        const stringName = builder.createString(name || "unknown");
        const stringShip = builder.createString(ship || "ship_gray");
        const stringToken = builder.createString(token || "");

        this.fb.NetSpawn.startNetSpawn(builder);
        this.fb.NetSpawn.addColor(builder, stringColor);
        this.fb.NetSpawn.addName(builder, stringName);
        this.fb.NetSpawn.addShip(builder, stringShip);
        this.fb.NetSpawn.addToken(builder, stringToken);
        const spawn = this.fb.NetSpawn.endNetSpawn(builder);

        this.fb.NetQuantum.startNetQuantum(builder);
        this.fb.NetQuantum.addMessageType(builder, this.fb.AllMessages.NetSpawn);
        this.fb.NetQuantum.addMessage(builder, spawn);
        const quantum = this.fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
        console.log("spawned");
    }

    sendControl(angle, boost, shoot, x, y, spectateControl, customDataJson) {
        const builder = new (<any>flatbuffers).Builder(0);

        let spectateString = false;
        let customDataJsonString = false;

        if (spectateControl) spectateString = builder.createString(spectateControl);
        if (customDataJson) customDataJsonString = builder.createString(customDataJson);

        this.fb.NetControlInput.startNetControlInput(builder);
        this.fb.NetControlInput.addAngle(builder, angle);
        this.fb.NetControlInput.addBoost(builder, boost);
        this.fb.NetControlInput.addShoot(builder, shoot);
        this.fb.NetControlInput.addX(builder, x);
        this.fb.NetControlInput.addY(builder, y);
        if (spectateControl) this.fb.NetControlInput.addSpectateControl(builder, spectateString);
        if (customDataJson) this.fb.NetControlInput.addCustomData(builder, customDataJsonString);

        const input = this.fb.NetControlInput.endNetControlInput(builder);

        this.fb.NetQuantum.startNetQuantum(builder);
        this.fb.NetQuantum.addMessageType(builder, this.fb.AllMessages.NetControlInput);
        this.fb.NetQuantum.addMessage(builder, input);
        const quantum = this.fb.NetQuantum.endNetQuantum(builder);

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
        this.onConnected();

        if (this.reloading) window.location.reload();
    }

    onClose(event) {
        console.log("disconnected");
        this.connected = false;
        this.reloading = true;
        this.connect();
    }

    onMessage(event) {
        const data = new Uint8Array(event.data);
        const buf = new (<any>flatbuffers).ByteBuffer(data);

        this.statBytesDown += data.byteLength;

        const quantum = this.fb.NetQuantum.getRootAsNetQuantum(buf);

        const messageType = quantum.messageType();
        let message = null;

        switch (messageType) {
            case this.fb.AllMessages.NetWorldView:
                message = quantum.message(new this.fb.NetWorldView());

                this.onView(message);
                break;
            case this.fb.AllMessages.NetPing: // Ping
                if (this.pingSent) {
                    this.latency = performance.now() - this.pingSent;
                    if (this.latency > 0 && this.latency < this.minLatency) this.minLatency = this.latency;
                }

                break;
            case this.fb.AllMessages.NetEvent:
                message = quantum.message(new this.fb.NetEvent());

                var event = {
                    type: message.type(),
                    data: JSON.parse(message.data())
                };

				if (event.data.roles !== undefined) {
					window.discordData = event;
				}
				Controls.addSecretShips(event);

                break;
            case this.fb.AllMessages.NetLeaderboard:
                message = quantum.message(new this.fb.NetLeaderboard());

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
                        ModeData: JSON.parse(entry.modeData()) || { flagStatus: "home" }
                    });
                }

                const record = message.record();

                let recordModel = {
                    Name: "",
                    Color: "red",
                    Score: 0,
                    Token: false
                };

                if (record) {
                    recordModel = {
                        Name: record.name(),
                        Color: record.color(),
                        Score: record.score(),
                        Token: record.token()
                    };
                }
                this.onLeaderboard({
                    Type: message.type(),
                    Entries: entries,
                    Record: recordModel
                });

                break;
        }
    }
}
