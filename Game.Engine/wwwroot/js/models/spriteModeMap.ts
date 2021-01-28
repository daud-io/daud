import { parseScssIntoRules, parseCssIntoRules, queryProperties } from "../parser/parseTheme.js";
import { readFileSync } from "fs";

export function getDefaultSpriteModeMapRules(graphics) {
    switch (graphics) {
        case "low":
            return parseScssIntoRules(readFileSync(__dirname + `/spriteModeMap_low.scss`, "utf-8"));
        case "medium":
            return parseScssIntoRules(readFileSync(__dirname + `/spriteModeMap_medium.scss`, "utf-8"));
        default: // high
            return parseScssIntoRules(readFileSync(__dirname + `/spriteModeMap_high.scss`, "utf-8"));
    }
}
export var spriteModeMapRules = [parseScssIntoRules(readFileSync(__dirname + `/spriteModeMap_high.scss`, "utf-8"))];
