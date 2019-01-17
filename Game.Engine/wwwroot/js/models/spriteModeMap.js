export var spriteModeMap = {
    "bg": {
        texture: "bg",
        scaleFactor: 10
    },

    "fish": {
        modes: {
            default: [ "fish" ]
        }
    },

    "ship_gray": {
        modes: {
            default: [ "ship_gray" ]
        }
    },

    "ship0": {
        modes: {
            default: [ "ship0" ],
            weaponupgrade: ["ship0", "circles"],
            boost: [ "ship0", "thruster_cyan"],
            invulnerable: ["ship0", "obstacle"]
        },
        selector: "ship0"
    },
    
    "ship_cyan": {
        modes: {
            default: [ "ship_cyan" ],
            weaponupgrade: ["ship_cyan", "circles"],
            boost: [ "ship_cyan", "thruster_cyan"],
            invulnerable: ["ship_cyan", "obstacle"]
        },
        selector: "ship_cyan"
    },
    "ship_green": {
        modes: {
            default: [ "ship_green" ],
            weaponupgrade: ["ship_green", "circles"],
            boost: [ "ship_green", "thruster_green"],
            invulnerable: ["ship_green", "obstacle"]
        },
        selector: "ship_green"
    },
    "ship_orange": {
        modes: {
            default: [ "ship_orange" ],
            weaponupgrade: ["ship_orange", "circles"],
            boost: [ "ship_orange", "thruster_orange"],
            invulnerable: ["ship_orange", "obstacle"]
        },
        selector: "ship_orange"
    },
    "ship_pink": {
        modes: {
            default: [ "ship_pink" ],
            weaponupgrade: ["ship_pink", "circles"],
            boost: [ "ship_pink", "thruster_pink"],
            invulnerable: ["ship_pink", "obstacle"]
        },
        selector: "ship_pink"
    },
    "ship_red": {
        modes: {
            default: [ "ship_red" ],
            weaponupgrade: ["ship_red", "circles"],
            boost: [ "ship_red", "thruster_red"],
            invulnerable: ["ship_red", "obstacle"]
        },
        selector: "ship_red"
    },
    "ship_yellow": {
        modes: {
            default: [ "ship_yellow" ],
            weaponupgrade: ["ship_yellow", "circles"],
            boost: [ "ship_yellow", "thruster_yellow"],
            invulnerable: ["ship_yellow", "obstacle"]
        },
        selector: "ship_yellow"
    },

    "ship_secret": {
        modes: {
            default: [ "ship_secret" ],
            weaponupgrade: ["ship_secret", "circles"],
            boost: [ "ship_secret", "thruster_yellow"],
            invulnerable: ["ship_secret", "obstacle"]
        },
        selector: "ship_secret"
    },
    "ship_zed": {
        modes: {
            default: [ "ship_zed" ],
            weaponupgrade: ["ship_zed", "circles"],
            boost: [ "ship_zed", "thruster_red"],
            invulnerable: ["ship_zed", "obstacle"]
        },
        selector: "ship_zed"
    },

    "bullet": { modes: { default: [ "bullet" ] } },
    "bullet_cyan": { modes: { default: [ "bullet_cyan" ] } },
    "bullet_green": { modes: { default: [ "bullet_green" ] } },
    "bullet_orange": { modes: { default: [ "bullet_orange" ] } },
    "bullet_pink": { modes: { default: [ "bullet_pink" ] } },
    "bullet_red": { modes: { default: [ "bullet_red" ] } },
    "bullet_yellow": { modes: { default: [ "bullet_yellow" ] } },

    "obstacle": { modes: { default: [ "obstacle" ] } },
    "seeker": { modes: { default: [ "seeker" ] } },
    "seeker_pickup": { modes: { default: [ "seeker_pickup" ] } },


    "ctf_base": { modes: { default: [ "ctf_base" ] } },

    "ctf_flag_blue": { modes: { default: [ "ctf_flag_blue" ] } },
    "ctf_flag_red": { modes: { default: [ "ctf_flag_red" ] } }
};