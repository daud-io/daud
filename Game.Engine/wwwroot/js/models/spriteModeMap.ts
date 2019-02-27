export const spriteModeMap = {
    bg: {
        texture: "bg",
        speed: 0.4,
        additionalLayers: [{ texture: "bg3", speed: 0.6 }, { texture: "bg5", speed: 0.8 }, { texture: "bg7", speed: 1 }]
    },

    fish: {
        modes: {
            default: ["fish"]
        }
    },

    ship_gray: {
        modes: {
            default: ["ship_gray"]
        }
    },

    ship0: {
        modes: {
            default: ["ship0"],
            boost: ["thruster_cyan"],
            offenseupgrade: ["offenseupgrade"],
            defenseupgrade: ["defenseupgrade"],
            invulnerable: ["invulnerable"],
            shield: ["shield"]
        },
        selector: "ship0"
    },

    ship_cyan: {
        modes: {
            default: ["ship_cyan"],
            boost: ["thruster_cyan"],
            offenseupgrade: ["offenseupgrade"],
            defenseupgrade: ["defenseupgrade"],
            invulnerable: ["invulnerable"],
            shield: ["shield"]
        },
        selector: "ship_cyan"
    },
    ship_blue: {
        modes: {
            default: ["ship_blue"],
            boost: ["thruster_blue"],
            offenseupgrade: ["offenseupgrade"],
            defenseupgrade: ["defenseupgrade"],
            invulnerable: ["invulnerable"],
            shield: ["shield"]
        },
        selector: "ship_blue"
    },
    ship_green: {
        modes: {
            default: ["ship_green"],
            boost: ["thruster_green"],
            offenseupgrade: ["offenseupgrade"],
            defenseupgrade: ["defenseupgrade"],
            invulnerable: ["invulnerable"],
            shield: ["shield"]
        },
        selector: "ship_green"
    },
    ship_orange: {
        modes: {
            default: ["ship_orange"],
            boost: ["thruster_orange"],
            offenseupgrade: ["offenseupgrade"],
            defenseupgrade: ["defenseupgrade"],
            invulnerable: ["invulnerable"],
            shield: ["shield"]
        },
        selector: "ship_orange"
    },
    ship_pink: {
        modes: {
            default: ["ship_pink"],
            boost: ["thruster_pink"],
            offenseupgrade: ["offenseupgrade"],
            defenseupgrade: ["defenseupgrade"],
            invulnerable: ["invulnerable"],
            shield: ["shield"]
        },
        selector: "ship_pink"
    },
    ship_red: {
        modes: {
            default: ["ship_red"],
            boost: ["thruster_red"],
            offenseupgrade: ["offenseupgrade"],
            defenseupgrade: ["defenseupgrade"],
            invulnerable: ["invulnerable"],
            shield: ["shield"]
        },
        selector: "ship_red"
    },
    ship_yellow: {
        modes: {
            default: ["ship_yellow"],
            boost: ["thruster_yellow"],
            offenseupgrade: ["offenseupgrade"],
            defenseupgrade: ["defenseupgrade"],
            invulnerable: ["invulnerable"],
            shield: ["shield"]
        },
        selector: "ship_yellow"
    },

    ship_secret: {
        modes: {
            default: ["ship_secret"],
            boost: ["thruster_cyan"],
            offenseupgrade: ["offenseupgrade"],
            defenseupgrade: ["defenseupgrade"],
            invulnerable: ["invulnerable"],
            shield: ["shield"]
        },
        selector: "ship_secret"
    },
    ship_zed: {
        modes: {
            default: ["ship_zed"],
            boost: ["thruster_cyan"],
            offenseupgrade: ["offenseupgrade"],
            defenseupgrade: ["defenseupgrade"],
            invulnerable: ["invulnerable"],
            shield: ["shield"]
        },
        selector: "ship_zed"
    },

    bullet: { modes: { default: ["bullet"] } },
    bullet_cyan: { modes: { default: ["bullet_cyan"] } },
    bullet_blue: { modes: { default: ["bullet_blue"] } },
    bullet_green: { modes: { default: ["bullet_green"] } },
    bullet_orange: { modes: { default: ["bullet_orange"] } },
    bullet_pink: { modes: { default: ["bullet_pink"] } },
    bullet_red: { modes: { default: ["bullet_red"] } },
    bullet_yellow: { modes: { default: ["bullet_yellow"] } },

    obstacle: { modes: { default: ["obstacle"] } },
    wormhole: { modes: { default: ["wormhole"] } },
    seeker: { modes: { default: ["seeker"] } },
    seeker_pickup: { modes: { default: ["seeker_pickup"] } },

    shield_pickup: { modes: { default: ["shield_pickup"] } },

    ctf_base: { modes: { default: ["ctf_base"] } },

    ctf_flag_blue: { modes: { default: ["ctf_flag_blue"] } },
    ctf_flag_red: { modes: { default: ["ctf_flag_red"] } },

    map: { modes: { default: ["map"] } }
};
