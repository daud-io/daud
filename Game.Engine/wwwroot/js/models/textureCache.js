export const textureCache = {};

textureCache.clear = function() {
    for (const key in textureCache) if (key != "clear") delete textureCache[key];
};
