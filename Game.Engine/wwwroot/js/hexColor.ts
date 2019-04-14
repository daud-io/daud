const isValidHex: (hex: string) => boolean = hex => /^#([A-Fa-f0-9]{3,4}){1,2}$/.test(hex);

const getChunksFromString: (str: string, chunkSize: number) => string[] | null = (str, chunkSize) => str.match(new RegExp(`.{${chunkSize}}`, "g"));

const convertHexUnitTo256: (hexStr: string) => number = hexStr => parseInt(hexStr.repeat(2 / hexStr.length), 16);

const getAlphafloat: (a?: number, alpha?: number) => number = (a, alpha) => {
    if (a) {
        return a / 256;
    }
    if (alpha) {
        if (1 < alpha && alpha <= 100) {
            return alpha / 100;
        }
        if (0 <= alpha && alpha <= 1) {
            return alpha;
        }
    }
    return 1;
};

export const hexToRGB: (hex: string, alpha?: number) => number[] = (hex, alpha) => {
    if (!isValidHex(hex)) {
        throw new Error("Invalid HEX");
    }
    const chunkSize = Math.floor((hex.length - 1) / 3);
    const hexArr = getChunksFromString(hex.slice(1), chunkSize) as string[];
    const [r, g, b, a] = hexArr.map(convertHexUnitTo256);
    return [r, g, b, getAlphafloat(a, alpha)];
};
