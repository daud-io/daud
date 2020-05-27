﻿import { parseScssIntoRules } from "../parser/parseTheme.js";

const text = `/*export const BLEND_MODES = {
    NORMAL:         0;
    ADD:            1;
    MULTIPLY:       2;
    SCREEN:         3;
    OVERLAY:        4;
    DARKEN:         5;
    LIGHTEN:        6;
    COLOR_DODGE:    7;
    COLOR_BURN:     8;
    HARD_LIGHT:     9;
    SOFT_LIGHT:     10;
    DIFFERENCE:     11;
    EXCLUSION:      12;
    HUE:            13;
    SATURATION:     14;
    COLOR:          15;
    LUMINOSITY:     16;
    NORMAL_NPM:     17;
    ADD_NPM:        18;
    SCREEN_NPM:     19;
};
*/

bg {
  file: "bg";
  scale: 5;
}


// fish
fish {
  file: "fish";
  scale: 0.002;
}


ship0,
ship_gray,
ship_flash,
ship_cyan,
ship_blue,
ship_green,
ship_orange,
ship_pink,
ship_red,
ship_yellow,
ship_secret,
ship_zed {
  scale: 0.0025;
}
// daudelins
ship0 {
  file: "ship0";
}
// abandoned ships
ship_gray {
  file: "ship_gray";
}
ship_flash {
  file: "ship_flash";
}
// default selectable ships
ship_cyan {
  file: "ship_cyan";
}
ship_blue {
  file: "ship_blue";
}
ship_green {
  file: "ship_green";
}
ship_orange {
  file: "ship_orange";
}
ship_pink {
  file: "ship_pink";
}
ship_red {
  file: "ship_red";
}
ship_yellow {
  file: "ship_yellow";
}
// premium ships
ship_secret {
  file: "ship_secret";
}
ship_zed {
  file: "ship_zed";
}
bullet,
bullet_cyan,
bullet_blue,
bullet_green,
bullet_orange,
bullet_pink,
bullet_red,
bullet_yellow {
  scale: 0.015625;
}
bullet {
  file: "bullet";
}
bullet_cyan {
  file: "bullet_cyan";
}
bullet_blue {
  file: "bullet_blue";
}
bullet_green {
  file: "bullet_green";
}
bullet_orange {
  file: "bullet_orange";
}
bullet_pink {
  file: "bullet_pink";
}
bullet_red {
  file: "bullet_red";
}
bullet_yellow {
  file: "bullet_yellow";
}

obstacle {
  file: "obstacle";
  scale: 0.0028;
}
seeker {
  file: "seeker";
  scale: 0.0025;
}
seeker_pickup {
  file: "seeker_pickup";
  scale: 0.02;
}

shield,
invulnerable {
  file: "shield";
  animated: true;
  loop: true;
  animation-speed: 0.5;
  size: 2.56;
  tile: {
    size: 64;
    count: 90;
  }
}

shield_pickup {
  file: "shield_pickup";
  animated: true;
  loop: true;
  animation-speed: 0.5;
  size: 2.56;
  tile: {
    size: 64;
    count: 30;
  }
}

wormhole {
  file: "wormhole";
  animated: true;
  loop: true;
  animation-speed: 0.5;
  size: 5.12;
  tile: {
    size: 128;
    count: 90;
  }
}

thruster_cyan,
thruster_blue,
thruster_green,
thruster_orange,
thruster_pink,
thruster_red,
thruster_yellow {
  animated: true;
  loop: false;
  animation-speed: 1;
  offset: {
    x: -145;
    y: 0;
  }
  size: 2.56;
  rotate: 6;
  tile: {
    size: 64;
    count: 29;
  }
}

thruster_cyan {
  file: "thruster_cyan";
}
thruster_blue {
  file: "thruster_blue";
}
thruster_green {
  file: "thruster_green";
}
thruster_orange {
  file: "thruster_orange";
}
thruster_pink {
  file: "thruster_pink";
}
thruster_red {
  file: "thruster_red";
}
thruster_yellow {
  file: "thruster_yellow";
}
offenseupgrade,
defenseupgrade {
  file: "circles";
  animated: true;
  loop: true;

  size: 3.6;

  tile: {
    size: 72;
    count: 11;
  }
}

offenseupgrade {
  animation-speed: 0.2;
  tint: 16711680;
}

defenseupgrade {
  animation-speed: 0.4;
  tint: 6711039;
}

arrow {
  file: "arrow";
  size: 100%;
}

// Capture the Flag
ctf_base {
  file: "ctf_base";
  size:3.072;
}
ctf_flag_red,
ctf_flag_blue {
  animated: true;
  tile: {
    size: 128;
    count: 6;
  }
  animation-speed: 0.1;
  size: 1.92;
  loop: true;
}
ctf_flag_blue {
  file: "ctf_flag_blue";
}
ctf_flag_red {
  file: "ctf_flag_red";
}
ctf_score_final {
  file: "ctf_score_final";
  size: 100%;
}
ctf_score_final_blue {
  file: "ctf_score_final_blue";
  size: 100%;
}
ctf_score_final_red {
  file: "ctf_score_final_red";
  size: 100%;
}
ctf_score_left_0 {
  file: "ctf_score_left_0";
  size: 100%;
}
ctf_score_left_1 {
  file: "ctf_score_left_1";
  size: 100%;
}
ctf_score_left_2 {
  file: "ctf_score_left_2";
  size: 100%;
}
ctf_score_left_3 {
  file: "ctf_score_left_3";
  size: 100%;
}
ctf_score_left_4 {
  file: "ctf_score_left_4";
  size: 100%;
}
ctf_score_right_0 {
  file: "ctf_score_right_0";
  size: 100%;
}
ctf_score_right_1 {
  file: "ctf_score_right_1";
  size: 100%;
}
ctf_score_right_2 {
  file: "ctf_score_right_2";
  size: 100%;
}
ctf_score_right_3 {
  file: "ctf_score_right_3";
  size: 100%;
}
ctf_score_right_4 {
  file: "ctf_score_right_4";
  size: 100%;
}
ctf_score_stripes {
  file: "ctf_score_stripes";
  size: 100%;
}
ctf_arrow_red {
  file: "ctf_arrow_red";
  size: 25.6;
}
ctf_arrow_blue {
  file: "ctf_arrow_blue";
  size: 25.6;
}
ctf_arrow_trans_flag {
  file: "ctf_arrow_trans_flag";
  size: 51.2;
}

map_beach {
  file: "beach_tileset";
  map: true;
  tile-height: 16;
  tile-width: 16;
  tile-count: 936;
  image-width: 576;
  image-height: 416;
  size: 27.248;
}
map_wilds {
  file: "buch-outdoor";
  map: true;
  tile: {
    width: 16;
    height: 16;
    count: 288;
    space: {
      height: 400;
      width: 400;
    }
  }
  image: {
    width: 384;
    height: 192;
  }
  size: 33.536;
}
map {
  file: "spritesheet";
  map: true;
  tile: {
    width: 64;
    height: 64;
    count: 81;
    space: {
      height: 400;
      width: 400;
    }
  }
  image: {
    width: 1024;
    height: 1024;
  }
  size:67.072;
}`;

export function getDefaultTextureMapRules() {
    return parseScssIntoRules(text);
}
export const textureMapRules = [parseScssIntoRules(text)];
