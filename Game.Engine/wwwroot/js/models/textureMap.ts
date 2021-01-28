import { parseScssIntoRules, parseCssIntoRules, queryProperties } from "../parser/parseTheme.js";
import { readFileSync } from "fs";
import { Settings } from "../settings";

export function getDefaultTextureMapRules(graphics) {
    switch (graphics) {
        case "low":
            return parseScssIntoRules(readFileSync(__dirname + `/textureMap_low.scss`, "utf-8"));
        case "medium":
            return parseScssIntoRules(readFileSync(__dirname + `/textureMap_medium.scss`, "utf-8"));
        default: // high
            return parseScssIntoRules(readFileSync(__dirname + `/textureMap_high.scss`, "utf-8"));
    }
}
export var textureMapRules = [parseScssIntoRules(readFileSync(__dirname + `/textureMap_high.scss`, "utf-8"))];
