/*export const BLEND_MODES = {
    NORMAL:         0,
    ADD:            1,
    MULTIPLY:       2,
    SCREEN:         3,
    OVERLAY:        4,
    DARKEN:         5,
    LIGHTEN:        6,
    COLOR_DODGE:    7,
    COLOR_BURN:     8,
    HARD_LIGHT:     9,
    SOFT_LIGHT:     10,
    DIFFERENCE:     11,
    EXCLUSION:      12,
    HUE:            13,
    SATURATION:     14,
    COLOR:          15,
    LUMINOSITY:     16,
    NORMAL_NPM:     17,
    ADD_NPM:        18,
    SCREEN_NPM:     19,
};
*/

export const textureMap = {
    bg: { file: "bg", scale: 4 },
    bg2:{file:"bg2",scale:5},
    bg3:{file:"bg2",scale:6},
    bg4:{file:"bg2",scale:7},
    bg5:{file:"bg2",scale:8},
    bg6:{file:"bg2",scale:9},
    bg7:{file:"bg2",scale:10},

    // fish
    fish: { file: "ship0", scale: 0.01 },

    // daudelins
    ship0: { file: "ship0", scale: 0.02 },

    // abandoned ships
    ship_gray: { file: "ship_gray", scale: 0.02 },
    ship_flash: { file: "ship_flash", scale: 0.02 },

    // default selectable ships
    ship_cyan: { file: "ship_cyan", scale: 0.02 },
    ship_green: { file: "ship_green", scale: 0.02 },
    ship_orange: { file: "ship_orange", scale: 0.02 },
    ship_pink: { file: "ship_pink", scale: 0.02 },
    ship_red: { file: "ship_red", scale: 0.02 },
    ship_yellow: { file: "ship_yellow", scale: 0.02 },

    // premium ships
    ship_secret: { file: "ship_secret", scale: 0.02 },
    ship_zed: { file: "ship_zed", scale: 0.02 },

    bullet: { file: "bullet", scale: 0.03 },
    bullet_cyan: { file: "bullet_cyan", scale: 0.03 },
    bullet_green: { file: "bullet_green", scale: 0.03 },
    bullet_orange: { file: "bullet_orange", scale: 0.03 },
    bullet_pink: { file: "bullet_pink", scale: 0.03 },
    bullet_red: { file: "bullet_red", scale: 0.03 },
    bullet_yellow: { file: "bullet_yellow", scale: 0.03 },

    obstacle: { file: "obstacle", scale: 0.0028 },
    seeker: { file: "seeker", scale: 0.02 },
    seeker_pickup: { file: "seeker_pickup", scale: 0.02 },

    shield: {
        file: "shield",
        animated: true,
        loop: true,
        animationSpeed: 0.5,
        scale: 0.04,
        tileSize: 64,
        tileCount: 90
    },

    invulnerable: {
        file: "shield",
        animated: true,
        loop: true,
        animationSpeed: 0.5,
        scale: 0.04,
        tileSize: 64,
        tileCount: 90
    },

    shield_pickup: {
        file: "shield_pickup",
        animated: true,
        loop: true,
        animationSpeed: 0.5,
        scale: 0.04,
        tileSize: 64,
        tileCount: 30
    },

    wormhole: {
        file: "wormhole",
        animated: true,
        loop: true,
        animationSpeed: 0.5,
        scale: 0.04,
        tileSize: 128,
        tileCount: 90
    },

    thruster_cyan: {
        file: "thruster_cyan",
        animated: true,
        loop: false,
        animationSpeed: 1.0,
        offset: { x: -145, y: 0 },
        scale: 0.04,
        rotate: 6,
        tileSize: 64,
        tileCount: 29
    },
    thruster_green: {
        file: "thruster_green",
        animated: true,
        loop: false,
        animationSpeed: 1.0,
        offset: { x: -145, y: 0 },
        scale: 0.04,
        rotate: 6,
        tileSize: 64,
        tileCount: 29
    },
    thruster_orange: {
        file: "thruster_orange",
        animated: true,
        loop: false,
        animationSpeed: 1.0,
        offset: { x: -145, y: 0 },
        scale: 0.04,
        rotate: 6,
        tileSize: 64,
        tileCount: 29
    },
    thruster_pink: {
        file: "thruster_pink",
        animated: true,
        loop: false,
        animationSpeed: 1.0,
        offset: { x: -145, y: 0 },
        scale: 0.04,
        rotate: 6,
        tileSize: 64,
        tileCount: 29
    },
    thruster_red: {
        file: "thruster_red",
        animated: true,
        loop: false,
        animationSpeed: 1.0,
        offset: { x: -145, y: 0 },
        scale: 0.04,
        rotate: 6,
        tileSize: 64,
        tileCount: 29
    },
    thruster_yellow: {
        file: "thruster_yellow",
        animated: true,
        loop: false,
        animationSpeed: 1.0,
        offset: { x: -145, y: 0 },
        scale: 0.04,
        rotate: 6,
        tileSize: 64,
        tileCount: 29
    },

    offenseupgrade: {
        file: "circles",
        animated: true,
        loop: true,
        animationSpeed: 0.2,
        scale: 0.05,
        tint: 16711680,
        tileSize: 72,
        tileCount: 11
    },

    defenseupgrade: {
        file: "circles",
        animated: true,
        loop: true,
        animationSpeed: 0.4,
        scale: 0.05,
        tint: 6711039,
        tileSize: 72,
        tileCount: 11
    },

    arrow: { file: "arrow", scale: 1 },

    // Capture the Flag
    ctf_base: { file: "ctf_base", scale: 0.003 },
    ctf_flag_blue: {
        file: "ctf_flag_blue",
        animated: true,
        tileSize: 128,
        tileCount: 6,
        animationSpeed: 0.1,
        scale: 0.015,
        loop: true
    },
    ctf_flag_red: {
        file: "ctf_flag_red",
        animated: true,
        tileSize: 128,
        tileCount: 6,
        animationSpeed: 0.1,
        scale: 0.015,
        loop: true
    },
    ctf_score_final: { file: "ctf_score_final", scale: 1 },
    ctf_score_final_blue: { file: "ctf_score_final_blue", scale: 1 },
    ctf_score_final_red: { file: "ctf_score_final_red", scale: 1 },
    ctf_score_left_0: { file: "ctf_score_left_0", scale: 1 },
    ctf_score_left_1: { file: "ctf_score_left_1", scale: 1 },
    ctf_score_left_2: { file: "ctf_score_left_2", scale: 1 },
    ctf_score_left_3: { file: "ctf_score_left_3", scale: 1 },
    ctf_score_left_4: { file: "ctf_score_left_4", scale: 1 },
    ctf_score_right_0: { file: "ctf_score_right_0", scale: 1 },
    ctf_score_right_1: { file: "ctf_score_right_1", scale: 1 },
    ctf_score_right_2: { file: "ctf_score_right_2", scale: 1 },
    ctf_score_right_3: { file: "ctf_score_right_3", scale: 1 },
    ctf_score_right_4: { file: "ctf_score_right_4", scale: 1 },
    ctf_score_stripes: { file: "ctf_score_stripes", scale: 1 },
    ctf_arrow_red: { file: "ctf_arrow_red", scale: 0.05 },
    ctf_arrow_blue: { file: "ctf_arrow_blue", scale: 0.05 },
    ctf_arrow_trans_flag: { file: "ctf_arrow_trans_flag", scale: 0.1 },

    map_beach: {
        file: "beach_tileset",
        map: true,
        tileHeight: 16,
        tileWidth: 16,
        tileCount: 936,
        imageWidth: 576,
        imageHeight: 416,
        scale: 0.0655
    },
    map_wilds: {
        file: "buch-outdoor",
        map: true,
        tileHeight: 16,
        tileWidth: 16,
        tileCount: 288,
        imageWidth: 384,
        imageHeight: 192,
        scale: 0.0655,
        tileSpaceHeight: 400,
        tileSpaceWidth: 400
    },
    map: {
        file: "spritesheet",
        map: true,
        tileHeight: 64,
        tileWidth: 64,
        tileCount: 81,
        imageWidth: 1024,
        imageHeight: 1024,
        scale: 0.0655,
        tileSpaceHeight: 400,
        tileSpaceWidth: 400
    }
};
