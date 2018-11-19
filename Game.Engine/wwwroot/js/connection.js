import { flatbuffers } from "./flatbuffers";
import { Game } from "./game_generated";

export class Connection {
    constructor() {
        this.onView = function(view) {};
        this.onLeaderboard = function(leaderboard) {};
        this.onConnected = function() {};
        this.reloading = false;
        this.connected = false;
        this.connect();

        this.statBytesUp = 0;
        this.statBytesDown = 0;
        this.statBytesDownPerSecond = 0;
        this.statBytesUpPerSecond = 0;

        var self = this;
        this.fb = Game.Engine.Networking.FlatBuffers;
        this.latency = 0;
        this.minLatency = 999;
        this.simulateLatency = 0;

        setInterval(function() {
            if (self.connected) {
                self.sendPing();
            }

            self.statBytesDownPerSecond = self.statBytesDown;
            self.statBytesUpPerSecond = self.statBytesUp;

            self.statBytesUp = 0;
            self.statBytesDown = 0;
        }, 1000);
    }

    connect() {
        var url;
        if (window.location.protocol === "https:") {
            url = "wss:";
        } else {
            url = "ws:";
        }
        url += "//" + window.location.host;
        url += "/api/v1/connect";

        this.socket = new WebSocket(url);
        this.socket.binaryType = "arraybuffer";

        var self = this;

        this.socket.onmessage = function(event) {
            if (self.simulateLatency > 0) {
                setTimeout(function() {
                    self.onMessage(event);
                }, self.simulateLatency);
            } else self.onMessage(event);
        };
        this.socket.onopen = function(event) {
            self.onOpen(event);
        };
        this.socket.onclose = function(event) {
            self.onClose(event);
        };
    }

    sendHook(hook) {
        //this.send(hook);
    }

    sendPing() {
        var builder = new flatbuffers.Builder(0);

        this.fb.NetPing.startNetPing(builder);

        this.pingSent = performance.now();

        //this.fb.Ping.addTime(builder, this.pingSent);
        var ping = this.fb.NetPing.endNetPing(builder);

        this.fb.NetQuantum.startNetQuantum(builder);
        this.fb.NetQuantum.addMessageType(builder, this.fb.AllMessages.NetPing);
        this.fb.NetQuantum.addMessage(builder, ping);
        var quantum = this.fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
    }

    sendSpawn(name, color, ship) {
        var builder = new flatbuffers.Builder(0);

        var stringColor = builder.createString(color || "gray");
        var stringName = builder.createString(name || "unknown");
        var stringShip = builder.createString(ship || "ship_gray");

        this.fb.NetSpawn.startNetSpawn(builder);
        this.fb.NetSpawn.addColor(builder, stringColor);
        this.fb.NetSpawn.addName(builder, stringName);
        this.fb.NetSpawn.addShip(builder, stringShip);
        var spawn = this.fb.NetSpawn.endNetSpawn(builder);

        this.fb.NetQuantum.startNetQuantum(builder);
        this.fb.NetQuantum.addMessageType(builder, this.fb.AllMessages.NetSpawn);
        this.fb.NetQuantum.addMessage(builder, spawn);
        var quantum = this.fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
        console.log("spawned");
    }

    sendControl(angle, boost, shoot, x, y) {
        var builder = new flatbuffers.Builder(0);

        this.fb.NetControlInput.startNetControlInput(builder);
        this.fb.NetControlInput.addAngle(builder, angle);
        this.fb.NetControlInput.addBoost(builder, boost);
        this.fb.NetControlInput.addShoot(builder, shoot);
        this.fb.NetControlInput.addX(builder, x);
        this.fb.NetControlInput.addY(builder, y);

        var input = this.fb.NetControlInput.endNetControlInput(builder);

        this.fb.NetQuantum.startNetQuantum(builder);
        this.fb.NetQuantum.addMessageType(builder, this.fb.AllMessages.NetControlInput);
        this.fb.NetQuantum.addMessage(builder, input);
        var quantum = this.fb.NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
    }

    send(databuffer) {
        if (this.socket.readyState === 1) {
            var self = this;
            if (this.simulateLatency > 0) {
                setTimeout(function() {
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
        var data = new Uint8Array(event.data);
        var buf = new flatbuffers.ByteBuffer(data);

        this.statBytesDown += data.byteLength;

        var quantum = this.fb.NetQuantum.getRootAsNetQuantum(buf);

        var messageType = quantum.messageType();
        var message = false;

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
            case this.fb.AllMessages.NetLeaderboard:
                message = quantum.message(new this.fb.NetLeaderboard());

                var entriesLength = message.entriesLength();
                var entries = [];
                for (var i = 0; i < entriesLength; i++) {
                    var entry = message.entries(i);

                    entries.push({
                        Name: entry.name(),
                        Color: entry.color(),
                        Score: entry.score(),
                        Position: {
                            X: entry.position().x(),
                            Y: entry.position().y()
                        }
                    });
                }

                var record = message.record();
                this.onLeaderboard({
                    Type: message.type(),
                    Entries: entries,
                    Record: {
                        Name: record.name(),
                        Color: record.color(),
                        Score: record.score()
                    }
                });
                break;
        }
    }
}
