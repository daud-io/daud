import { parseScssIntoRules, parseCssIntoRules, queryProperties } from "../parser/parseTheme.js";
import { readFileSync } from 'fs';

export function getDefaultTextureMapRules(){return parseScssIntoRules(readFileSync(__dirname + '/textureMap.scss', 'utf-8'));}
export var textureMapRules = [parseScssIntoRules(readFileSync(__dirname + '/textureMap.scss', 'utf-8'))];
