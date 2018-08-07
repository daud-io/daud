(function () {
    var Connection = function() {

        this.onView = function (view) { };

        this.reloading = false;
        this.connected = false;
        this.connect();

        var self = this;
        setInterval(function () {
            if (self.connected) {
                self.sendPing();
            }
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
        sendPing: function () {
            this.send({ Type: 1 });
            this.pingSent = performance.now();
        },
        sendSpawn: function (name) {
            this.send({ Type: 2, Name: name });
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
                this.socket.send(JSON.stringify(obj));
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
            var message = JSON.parse(json);

            switch (message.Type) {
                case 3: // View
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