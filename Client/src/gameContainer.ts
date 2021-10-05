import {
    Color3,
    Color4,
    Engine,
    FreeCamera,
    HemisphericLight,
    Scene,
    Vector2,
    Vector3,
} from "@babylonjs/core";
import { Loader } from "./loader";
import { AdvancedDynamicTexture } from "@babylonjs/gui";
import { Leaderboard } from "./leaderboard";
import { WorldMeshLoader } from "./worldMeshLoader";
import { Sounds } from "./sounds";
import { Cache } from "./cache";
import * as bus from "./bus";
import { Connection } from "./connection";

export class GameContainer {
    scene: Scene;
    readonly engine: Engine;
    readonly leaderboard: Leaderboard;
    readonly worldMeshLoader: WorldMeshLoader;
    readonly sounds: Sounds;
    readonly baseURL: string;
    camera: FreeCamera;
    cameraPosition: Vector2 = Vector2.Zero();
    cameraHeight: number = 0;

    readonly guiTexture: AdvancedDynamicTexture;
    readonly loader: Loader;
    ready: boolean = false;
    fleetID: number = 0;

    readonly cache: Cache;
    updateCounter: number = 0;
    viewCounter: number = 0;
    canvas: HTMLCanvasElement;

    connection: Connection;

    constructor(canvas: HTMLCanvasElement, connection: Connection) {
        this.connection = connection;
        this.baseURL = document.body.attributes['data-static-url-base'];
        this.canvas = canvas;
        this.engine = new Engine(canvas, true);
        this.scene = new Scene(this.engine);
        this.scene.ambientColor = Color3.White();
        this.scene.clearColor = new Color4(0,0,0,1);
        this.loader = new Loader(this);
        this.worldMeshLoader = new WorldMeshLoader(this);
        this.cache = new Cache(this);

        this.camera = this.setupCamera();
        this.guiTexture = this.setupGUI();
        this.leaderboard = new Leaderboard(this);
        this.sounds = new Sounds(this);

        this.positionCamera(Vector2.Zero());
        this.setupLights();

        bus.on("worldjoin", () => {
            while (this.scene.meshes.length) this.scene.meshes[0].dispose();
            let skipped=0;
            while (this.scene.rootNodes.length - skipped > 0)
            {
                const nextNode = this.scene.rootNodes[0+skipped];

                switch(nextNode.name)
                {
                    case "containerLight":
                    case "containerCamera":
                        skipped++;
                        break;
                    default:
                        nextNode.dispose();
                        break;
                }

            }
            this.resize();
            this.ready = false;
            this.scene.render();
        });

    }

    focus() {
        this.canvas.focus();
    }

    setupGUI(): AdvancedDynamicTexture {
        return AdvancedDynamicTexture.CreateFullscreenUI("UI");
    }

    setupCamera(): FreeCamera {
        this.cameraHeight = 4000;
        const camera = new FreeCamera("containerCamera", new Vector3(0, this.cameraHeight, 0), this.scene);
        camera.maxZ = 20000;
        camera.upVector = new Vector3(0, 0, 1);
        camera.setTarget(new Vector3(this.cameraPosition.x, 0, this.cameraPosition.y));

        return camera;
    }

    setupLights() {
        var light = new HemisphericLight("containerLight", new Vector3(0, 1, 0), this.scene);
        light.intensity *= 0.3;
    }

    positionCamera(serverPosition: Vector2) {
        this.cameraPosition.x = serverPosition.x * 0.2 + this.cameraPosition.x * 0.8;
        this.cameraPosition.y = serverPosition.y * 0.2 + this.cameraPosition.y * 0.8;
        this.camera.position = new Vector3(this.cameraPosition.x, this.cameraHeight, this.cameraPosition.y);

        //console.log(this.camera.position);
    }

    resize() {
        this.engine.resize();
    }
}
