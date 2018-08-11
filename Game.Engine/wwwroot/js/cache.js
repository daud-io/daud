(function () {
    var Cache = function () {
        this.bodies = {};
    }

    Cache.prototype = {
        update: function (updates, deletes) {

            // delete objects that should no longer exist
            for (var i = 0; i < deletes.length; i++) {
                var deleteKey = deletes[i];
                delete this.bodies['b-' + deleteKey];
            }

            // update objects that should be here
            for (var i = 0; i < updates.length; i++) {
                var update = updates[i];

                this.bodies['b-' + update.id] = update;
            }
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