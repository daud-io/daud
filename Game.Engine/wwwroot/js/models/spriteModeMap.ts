import { parseScssIntoRules, parseCssIntoRules, queryProperties } from "../parser/parseTheme.js";
import { readFileSync } from 'fs';
export function getDefaultSpriteModeMapRules(){return parseScssIntoRules(readFileSync(__dirname + '/spriteModeMap.scss', 'utf-8'));}
export var spriteModeMapRules = [parseScssIntoRules(readFileSync(__dirname + '/spriteModeMap.scss', 'utf-8'))];
