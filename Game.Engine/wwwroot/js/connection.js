(function () {
    var Connection = function() {

        this.onView = function (view) { };

        this.reloading = false;
        this.connect();

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
            console.log('connected');

            if (this.reloading)
                window.location.reload();

            this.sendSpawn();
        },
        onClose: function (event) {
            console.log('disconnected');

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
            }
        }
    };

    Game.Connection = Connection;
}).call(this);