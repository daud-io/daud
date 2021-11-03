import { Loader, SpriteDefinition, TextureDefinition } from "./loader";

import infoBase from "./assets/base.json";
import infoThemesOriginal from "./assets/original.json";

export class SpriteLibrary {
    sprites: Record<string, SpriteDefinition>;
    textures: Record<string, TextureDefinition>;
    name?: string;

    constructor(name: string) {
        this.sprites = {};
        this.textures = {};
        this.name = name;
    }

    static async load(name: string): Promise<SpriteLibrary> {

        const librarybase: SpriteLibrary = 
            name == 'assets/base' ? infoBase
            : name == 'assets/themes/original' ? infoThemesOriginal
            : await window
                .fetch(`/${name}/info.json`)
                .then((response) => response.json())
                .then(base => base as SpriteLibrary);

        librarybase.name = name;

        const sprites = librarybase.sprites;
        for (let spriteName in sprites) {
            let sprite = sprites[spriteName];
            if (sprite.extends) {
                sprites[spriteName] = Loader.merge(sprites[sprite.extends], sprite);
            }
        }

        const textures = librarybase.textures;
        for (let textureName in textures) {
            let texture = textures[textureName];

            if (texture.extends) {
                texture = Loader.merge(textures[texture.extends], texture);
                // preserve the local abstract value
                texture.abstract = textures[textureName].abstract;
                textures[textureName] = texture;
            }
            if (texture.url) {
                texture.url = `/${name}/${texture.url}`;
            } else {
                let x = 1;
            }
        }

        return librarybase;
    }
}

