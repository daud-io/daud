import { Color3, CubeTexture, DirectionalLight, Engine, FreeCamera, HemisphericLight, Matrix, Mesh, MeshBuilder, PointLight, Quaternion, Scene, SceneLoader, ShadowGenerator, StandardMaterial, Texture, Vector2, Vector3 } from "@babylonjs/core";
import { Loader } from "./loader";
import { AdvancedDynamicTexture } from "@babylonjs/gui";
import { Leaderboard } from "./leaderboard";
import { WorldMeshLoader } from "./worldMeshLoader";
import { Sounds } from "./sounds";
import { Cache } from "./cache";

export class GameContainer {
    readonly scene: Scene;
    readonly engine: Engine;
    readonly leaderboard: Leaderboard;
    readonly worldMeshLoader: WorldMeshLoader;
    readonly sounds: Sounds;
    readonly camera: FreeCamera;
    cameraPosition: Vector2 = Vector2.Zero();
    cameraHeight: number = 0;

    readonly guiTexture: AdvancedDynamicTexture;
    readonly loader: Loader;
    ready: boolean;
    fleetID: number = 0;

    readonly cache: Cache;
    updateCounter: number = 0;
    viewCounter:number = 0;
    canvas: HTMLCanvasElement;

    constructor(canvas: HTMLCanvasElement) {
        this.ready = false;
        this.canvas = canvas;
        this.engine = new Engine(canvas, true);
        this.scene = new Scene(this.engine);
        this.scene.ambientColor = Color3.White()
        this.loader = new Loader(this);
        this.worldMeshLoader = new WorldMeshLoader(this);
        this.cache = new Cache(this);

        this.camera = this.setupCamera();
        this.guiTexture = this.setupGUI();
        this.leaderboard = new Leaderboard(this);
        this.sounds = new Sounds(this);
        
        this.positionCamera(Vector2.Zero());
        this.setupLights();
    }

    focus()
    {
        this.canvas.focus();
    }

    setupGUI(): AdvancedDynamicTexture
    {
        return AdvancedDynamicTexture.CreateFullscreenUI("UI");
    }

    setupCamera(): FreeCamera
    {
        const camera = new FreeCamera("Camera", new Vector3(0, this.cameraHeight, 0), this.scene);
        camera.maxZ = 20000;
        return camera;
    }

    setupLights()
    {
        var light = new HemisphericLight("containerLight", new Vector3(0, 1, 0), this.scene);
        light.intensity *= 0.3;
    }

    positionCamera(serverPosition: Vector2)
    {
        this.cameraHeight = 4000;
        this.cameraPosition.x = serverPosition.x * 0.2 + this.cameraPosition.x * 0.8;
        this.cameraPosition.y = serverPosition.y * 0.2 + this.cameraPosition.y * 0.8;
        this.camera.position = new Vector3(this.cameraPosition.x, this.cameraHeight, this.cameraPosition.y);
        this.camera.upVector = new Vector3(0, 0, 1);
        this.camera.setTarget(new Vector3(this.cameraPosition.x, 0, this.cameraPosition.y));

        //console.log(this.camera.position);
        this.ready = true;
    }

    resize() {
        this.engine.resize();
    }
}
