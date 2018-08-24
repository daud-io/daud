(function () {
    var Cache = function () {
        this.bodies = {};
        this.count = 0;
    }

    Cache.prototype = {
        update: function (updates, deletes) {

            // delete objects that should no longer exist
            for (var i = 0; i < deletes.length; i++) {
                var deleteKey = deletes[i];
                delete this.bodies['b-' + deleteKey];
                console.log('deleted: ' + deleteKey);
                this.count--;
            }

            // update objects that should be here
            for (var i = 0; i < updates.length; i++) {
                var update = updates[i];
                var existing = this.bodies['b-' + update.ID];
                this.bodies['b-' + update.ID] = update;

                if (!existing)
                    this.count++;
            }

            console.log(this.bodies);
        },
        foreach: function (action, thisObj) {

            for (var key in this.bodies) {
                if (key.indexOf('b-') == 0) {
                    action.apply(thisObj, [this.bodies[key]]);
                }
            }
        }
    };

    Game.Cache = Cache;
}).call(this);