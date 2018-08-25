(function () {
    var Connection = function() {

        this.onView = function (view) { };

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
            var self = this;

            this.socket.onmessage = function (event) { self.onMessage(event); };
            this.socket.onopen = function (event) { self.onOpen(event); };
            this.socket.onclose = function (event) { self.onClose(event); };

        },
        sendHook: function (hook) {
            this.send(hook);
        },
        sendPing: function () {
            this.send({ Type: 1 });
            this.pingSent = performance.now();
        },
        sendSpawn: function (name) {
            this.send({ Type: 2, Name: name });
            console.log('spawned');
        },
        sendControl: function (angle, boost, shoot, nick, ship) {
            this.send({
                Type: 4,
                Angle: angle,
                BoostRequested: boost,
                ShootRequested: shoot,
                Name: nick,
                Ship: ship
            });
        },
        send: function (obj) {
            if (this.socket.readyState === 1) {
                var s = JSON.stringify(obj);
                this.socket.send(s);

                this.statBytesUp += s.length;
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

            var json = event.data;
            this.statBytesDown += event.data.length;

            var message = JSON.parse(json);

            switch (message.Type) {
                case 3: // View
                    //console.log('view');
                    this.onView(message);
                    break;
                case 1: // Ping
                    this.latency = performance.now() - this.pingSent;
                    break;
            }
        }
    };

    Game.Connection = Connection;
}).call(this);