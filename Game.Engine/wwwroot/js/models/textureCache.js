export var textureCache = {};

textureCache.clear = function()
{
    for(var key in textureCache)
        if (key != 'clear')
            delete textureCache[key];
}
