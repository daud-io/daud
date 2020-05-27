import { parseScssIntoRules } from "../parser/parseTheme.js";

const text = `bg {
    layer-speeds: .4;
    layer-cpu-levels: 1;
    layer-textures: 'bg';
}


world {
    danger-color: #ff00001a;
    edge-color: #0000ff;
    edge-width:40;
    blue-color: #0000ff1a;
}

fish {
    textures: 'fish';
}

ship {
    textures: 'ship_gray';
    selector: 'ship_gray';

    &.gray {
        textures: 'ship_gray';
        selector: 'ship_gray';
    }


    &.secret {
        textures: 'ship_secret';
        selector: 'ship_secret';
    }

    &.zed {
        textures: 'ship_zed';
        selector: 'ship_zed';
    }

    &.cyan {
        textures: 'ship_cyan';
        selector: 'ship_cyan';
    }

    &.boost {
        textures: inherit, 'thruster_cyan';
    }

    &.blue {
        textures: 'ship_blue';
        selector: 'ship_blue';

        &.boost {
            textures: inherit, 'thruster_blue';
        }
    }

    &.green {
        textures: 'ship_green';
        selector: 'ship_green';

        &.boost {
            textures: inherit, 'thruster_green';
        }
    }

    &.orange {
        textures: 'ship_orange';
        selector: 'ship_orange';

        &.boost {
            textures: inherit, 'thruster_orange';
        }
    }

    &.pink {
        textures: 'ship_pink';
        selector: 'ship_pink';

        &.boost {
            textures: inherit, 'thruster_pink';
        }
    }

    &.red {
        textures: 'ship_red';
        selector: 'ship_red';

        &.boost {
            textures: inherit, 'thruster_red';
        }
    }

    &.yellow {
        textures: 'ship_yellow';
        selector: 'ship_yellow';

        &.boost {
            textures: inherit, 'thruster_yellow';
        }
    }

    &.offenseupgrade {
        textures: inherit, 'offenseupgrade';
    }

    &.defenseupgrade {
        textures: inherit, 'defenseupgrade';
    }

    &.invulnerable {
        textures: inherit, 'invulnerable';
    }

    &.shield {
        textures: inherit, 'shield';
    }
}

ship0 {
    textures: 'ship0';
    selector: 'ship0';
    
    &.boost {
        textures: inherit, 'thruster_cyan';
    }


    &.offenseupgrade {
        textures: inherit, 'offenseupgrade';
    }

    &.defenseupgrade {
        textures: inherit, 'defenseupgrade';
    }

    &.invulnerable {
        textures: inherit, 'invulnerable';
    }

    &.shield {
        textures: inherit, 'shield';
    }
}

bullet {
    textures: 'bullet';

    &.cyan {
        textures: 'bullet_cyan';
    }

    &.blue {
        textures: 'bullet_blue';
    }

    &.green {
        textures: 'bullet_green';
    }

    &.orange {
        textures: 'bullet_orange';
    }

    &.pink {
        textures: 'bullet_pink';
    }

    &.red {
        textures: 'bullet_red';
    }

    &.yellow {
        textures: 'bullet_yellow';
    }
}

obstacle {
    textures: 'obstacle';
}

wormhole {
    textures: 'wormhole';
}

seeker {
    textures: 'seeker';
}

seeker.pickup {
    textures: 'seeker_pickup';
}

shield.pickup {
    textures: 'shield_pickup';
}

ctf.base {
    textures: 'ctf_base';
}

ctf.flag.blue {
    textures: 'ctf_flag_blue';
}

ctf.flag.red {
    textures: 'ctf_flag_red';
}

map {
    textures: 'map';
}

boom {
    textures: 'boom';
}`;
export function getDefaultSpriteModeMapRules() {
    return parseScssIntoRules(text);
}
export const spriteModeMapRules = [parseScssIntoRules(text)];
