(function () {
    var Connection = function() {
        var self = this;
        var url;
        if (window.location.protocol === "https:") {
            url = "wss:";
        } else {
            url = "ws:";
        }
        url += "//" + window.location.host;
        url += "/api/v1/connect";

        this.socket = new WebSocket(url);
        this.onView = function (view) { };

        this.socket.onmessage = function (event) { self.onMessage(event); };
        this.socket.onopen = function (event) { self.onOpen(event); };
        this.socket.onclose = function (event) { self.onClose(event); };
    }

    Connection.prototype = {
        sendSpawn: function (name) {
            this.send({ Type: 2, Name: name });
        },
        sendSteering: function(angle) {
            this.send({ Type: 4, Angle: angle });
        },
        send: function (obj) {
            if (this.socket.readyState === 1) {
                this.socket.send(JSON.stringify(obj));
            }
        },
        onOpen: function (event) {
            console.log('connected');

            this.sendSpawn();
        },
        onClose: function (event) {
            console.log('disconnected');
        },
        onMessage: function (event) {
            var json = event.data;
            var message = JSON.parse(json);

            switch (message.Type) {
                case 3: // View
                    this.onView(message);
                    this.sendSteering(this.angle);
                    break;
            }
        }
    };

    Game.Connection = Connection;
}).call(this);