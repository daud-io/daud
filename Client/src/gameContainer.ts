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
import { Leaderboard } from "./leaderboard";
import { WorldMeshLoader } from "./worldMeshLoader";
import { Sounds } from "./sounds";
import { Cache } from "./cache";
import * as bus from "./bus";
import { Connection } from "./connection";
import { Reticle } from "./reticle";
import { HUD } from "./hud";
import { AllMessages } from "./daud-net/all-messages";
import { NetWorldView } from "./daud-net/net-world-view";
//import "@babylonjs/inspector";

export class GameContainer {
    scene: Scene;
    readonly engine: Engine;
    readonly leaderboard: Leaderboard;
    readonly worldMeshLoader: WorldMeshLoader;
    readonly sounds: Sounds;
    readonly baseURL: string = '/';

    readonly reticle: Reticle;
    readonly hud: HUD;

    camera: FreeCamera;
    cameraPosition: Vector2 = Vector2.Zero();
    cameraHeight: number = 0;

    readonly loader: Loader;
    ready: boolean = false;
    fleetID: number = 0;

    readonly cache: Cache;
    updateCounter: number = 0;
    viewCounter: number = 0;
    canvas: HTMLCanvasElement;

    connection: Connection;
    light?: HemisphericLight;
    boundingRect: DOMRect;
    pointerLocked: boolean = false;
    alive: boolean = false;
    touchscreen: boolean = false;
    backgrounded: boolean = false;

    constructor(canvas: HTMLCanvasElement, connection: Connection) {
        this.connection = connection;
        this.canvas = canvas;
        this.boundingRect = this.canvas.getBoundingClientRect();
        this.engine = new Engine(canvas, true);
        this.scene = new Scene(this.engine);
        this.scene.ambientColor = Color3.White();
        this.scene.clearColor = new Color4(0, 0, 0, 1);
        this.loader = new Loader(this);
        this.worldMeshLoader = new WorldMeshLoader(this);
        this.cache = new Cache(this);

        this.camera = this.setupCamera();
        this.leaderboard = new Leaderboard(this);
        this.sounds = new Sounds(this);

        this.positionCamera(Vector2.Zero());
        this.setupLights();

        this.reticle = new Reticle(this);
        this.hud = new HUD(this);

        //this.scene.debugLayer.show();

        bus.on("worldjoin", () => {
            while (this.scene.meshes.length) this.scene.meshes[0].dispose();
            let skipped = 0;
            while (this.scene.rootNodes.length - skipped > 0) {
                const nextNode = this.scene.rootNodes[0 + skipped];

                switch (nextNode.name) {
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

        bus.on("worldview", (newView: NetWorldView) => {
            const newAlive = newView.isalive();
            if (this.alive && !newAlive)
                bus.emit('dead');
            if (!this.alive && newAlive)
                bus.emit('alive');

            this.alive = newAlive;
        });
        
        bus.on("dead", () => {
            this.alive = false;
        });
        bus.emit("dead");

    }

    focus() {
        this.canvas.focus();
    }

    setupCamera(): FreeCamera {
        this.cameraHeight = 4000;
        const camera = new FreeCamera("containerCamera", new Vector3(0, this.cameraHeight, 0), this.scene);
        camera.maxZ = 20000;
        camera.upVector = new Vector3(0, 0, 1);
        camera.setTarget(new Vector3(this.cameraPosition.x, 0, this.cameraPosition.y));
        this.scene.detachControl();
        camera.detachControl();

        return camera;
    }

    setupLights() {
        this.light = new HemisphericLight("containerLight", new Vector3(0, 1, 0), this.scene);
        this.light.intensity *= 0.3;
    }

    positionCamera(newPosition: Vector2) {
        this.cameraPosition.x = newPosition.x * 0.2 + this.cameraPosition.x * 0.8;
        this.cameraPosition.y = newPosition.y * 0.2 + this.cameraPosition.y * 0.8;
        this.camera.position.set(this.cameraPosition.x, this.cameraHeight, this.cameraPosition.y);
    }

    resize() {
        console.log('resize');
        this.engine.resize();
        this.boundingRect = this.canvas.getBoundingClientRect();
        //this.scene.onKeyboardObservable.add((kbInfo, eventState) => this.onKey(kbInfo));
    }
}
