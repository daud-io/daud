(function () {
    var Connection = function() {

        this.onView = function (view) { };
        this.onLeaderboard = function (leaderboard) { };

        this.reloading = false;
        this.connected = false;
        this.connect();

        this.statBytesUp = 0;
        this.statBytesDown = 0;
        this.statBytesDownPerSecond = 0;
        this.statBytesUpPerSecond = 0;

        var self = this;
        setInterval(function () {
            if (self.connected) {
                self.sendPing();
            }

            self.statBytesDownPerSecond = self.statBytesDown;
            self.statBytesUpPerSecond = self.statBytesUp;

            self.statBytesUp = 0;
            self.statBytesDown = 0;
        }, 1000);
    }

    Connection.prototype = {

        connect: function () {
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

            this.socket.onmessage = function (event) { self.onMessage(event); };
            this.socket.onopen = function (event) { self.onOpen(event); };
            this.socket.onclose = function (event) { self.onClose(event); };

        },
        sendHook: function (hook) {
            //this.send(hook);
        },
        sendPing: function () {

            var builder = new flatbuffers.Builder(0);

            Game.Engine.Networking.FlatBuffers.NetPing.startNetPing(builder);

            this.pingSent = performance.now();

            //Game.Engine.Networking.FlatBuffers.Ping.addTime(builder, this.pingSent);
            var ping = Game.Engine.Networking.FlatBuffers.NetPing.endNetPing(builder);


            Game.Engine.Networking.FlatBuffers.NetQuantum.startNetQuantum(builder);
            Game.Engine.Networking.FlatBuffers.NetQuantum.addMessageType(builder, Game.Engine.Networking.FlatBuffers.AllMessages.NetPing);
            Game.Engine.Networking.FlatBuffers.NetQuantum.addMessage(builder, ping);
            var quantum = Game.Engine.Networking.FlatBuffers.NetQuantum.endNetQuantum(builder);

            builder.finish(quantum);

            this.send(builder.asUint8Array());
        },
        sendSpawn: function (name, color, ship) {

            var builder = new flatbuffers.Builder(0);

            var stringColor = builder.createString(color || "gray");
            var stringName = builder.createString(name || "unknown");
            var stringShip = builder.createString(ship || "ship_gray");

            Game.Engine.Networking.FlatBuffers.NetSpawn.startNetSpawn(builder);
            Game.Engine.Networking.FlatBuffers.NetSpawn.addColor(builder, stringColor);
            Game.Engine.Networking.FlatBuffers.NetSpawn.addName(builder, stringName);
            Game.Engine.Networking.FlatBuffers.NetSpawn.addShip(builder, stringShip);
            var spawn = Game.Engine.Networking.FlatBuffers.NetSpawn.endNetSpawn(builder);

            Game.Engine.Networking.FlatBuffers.NetQuantum.startNetQuantum(builder);
            Game.Engine.Networking.FlatBuffers.NetQuantum.addMessageType(builder, Game.Engine.Networking.FlatBuffers.AllMessages.NetSpawn);
            Game.Engine.Networking.FlatBuffers.NetQuantum.addMessage(builder, spawn);
            var quantum = Game.Engine.Networking.FlatBuffers.NetQuantum.endNetQuantum(builder);

            builder.finish(quantum);

            this.send(builder.asUint8Array());
            console.log('spawned');

        },
        sendControl: function (angle, boost, shoot) {
            var builder = new flatbuffers.Builder(0);

            Game.Engine.Networking.FlatBuffers.NetControlInput.startNetControlInput(builder);
            Game.Engine.Networking.FlatBuffers.NetControlInput.addAngle(builder, angle);
            Game.Engine.Networking.FlatBuffers.NetControlInput.addBoost(builder, boost);
            Game.Engine.Networking.FlatBuffers.NetControlInput.addShoot(builder, shoot);
            var input = Game.Engine.Networking.FlatBuffers.NetControlInput.endNetControlInput(builder);

            Game.Engine.Networking.FlatBuffers.NetQuantum.startNetQuantum(builder);
            Game.Engine.Networking.FlatBuffers.NetQuantum.addMessageType(builder, Game.Engine.Networking.FlatBuffers.AllMessages.NetControlInput);
            Game.Engine.Networking.FlatBuffers.NetQuantum.addMessage(builder, input);
            var quantum = Game.Engine.Networking.FlatBuffers.NetQuantum.endNetQuantum(builder);

            builder.finish(quantum);

            this.send(builder.asUint8Array());
        },
        send: function (databuffer) {
            if (this.socket.readyState === 1) {
                this.socket.send(databuffer);
                this.statBytesUp += databuffer.length;
            }
        },
        onOpen: function (event) {
            this.connected = true;
            console.log('connected');

            if (this.reloading)
                window.location.reload();
        },
        onClose: function (event) {
            console.log('disconnected');
            this.connected = false;
            this.reloading = true;
            this.connect();
        },
        onMessage: function (event) {

            var data = new Uint8Array(event.data);
            var buf = new flatbuffers.ByteBuffer(data);

            this.statBytesDown += data.byteLength;

            var quantum = Game.Engine.Networking.FlatBuffers.NetQuantum.getRootAsNetQuantum(buf);

            var messageType = quantum.messageType();

            switch (messageType) {
                case Game.Engine.Networking.FlatBuffers.AllMessages.NetWorldView:

                    var message = quantum.message(new Game.Engine.Networking.FlatBuffers.NetWorldView());

                    this.onView(message);
                    break;
                case Game.Engine.Networking.FlatBuffers.AllMessages.NetPing: // Ping
                    this.latency = performance.now() - this.pingSent;
                    break;
                case Game.Engine.Networking.FlatBuffers.AllMessages.NetLeaderboard:
                    var message = quantum.message(new Game.Engine.Networking.FlatBuffers.NetLeaderboard());

                    var entriesLength = message.entriesLength();
                    var entries = [];
                    for (var i = 0; i < entriesLength; i++) {
                        var entry = message.entries(i);

                        entries.push({
                            Name: entry.name(),
                            Color: entry.color(),
                            Score: entry.score()
                        });
                    }

                    this.onLeaderboard({
                        Type: message.type(),
                        Entries: entries
                    });
                    break;
            }
        }
    };

    Game.Connection = Connection;
}).call(this);