import { DirectionalLight, Engine, FreeCamera, HemisphericLight, Matrix, Mesh, MeshBuilder, PointLight, Scene, SceneLoader, StandardMaterial, Texture, Vector2, Vector3 } from "@babylonjs/core";
import { AdvancedDynamicTexture } from "@babylonjs/gui";

export class GameContainer {
    scene: Scene;
    engine: Engine;
    camera: FreeCamera;
    light: DirectionalLight;

    guiTexture: AdvancedDynamicTexture;
    cameraPosition: Vector2;
    cameraHeight: number;

    ready: boolean;

    constructor(canvas: HTMLCanvasElement) {
        this.ready = false;
        this.engine = new Engine(canvas, true);
        this.scene = new Scene(this.engine);
        this.cameraHeight = 4000;
        this.camera = new FreeCamera("Camera", new Vector3(0, this.cameraHeight, 0), this.scene);
        this.camera.setTarget(new Vector3(0,0,0));
        this.light = new DirectionalLight("DirectionalLight", new Vector3(1, -1, 1), this.scene);

        this.guiTexture = AdvancedDynamicTexture.CreateFullscreenUI("UI");
        
        this.cameraPosition = Vector2.Zero();

        //this.defineGround();
    }

    defineGround() {
        const groundMaterial = new StandardMaterial("groundmaterial", this.scene);

        groundMaterial.diffuseTexture = new Texture("assets/themes/original/bg2.png", this.scene);
        groundMaterial.specularTexture = groundMaterial.diffuseTexture;
        groundMaterial.emissiveTexture = groundMaterial.diffuseTexture;
        groundMaterial.ambientTexture = groundMaterial.diffuseTexture;

        const ground = MeshBuilder.CreateGround("Ground", { height: 30000, width: 30000, subdivisions: 4 });
        ground.position.y = -510;
        //ground.enablePointerMoveEvents = true;

        ground.material = groundMaterial;
    }

    PositionCamera(serverPosition: Vector2)
    {
        this.cameraPosition.x = serverPosition.x * 0.2 + this.cameraPosition.x * 0.8;
        this.cameraPosition.y = serverPosition.y * 0.2 + this.cameraPosition.y * 0.8;
        this.camera.position = new Vector3(this.cameraPosition.x, this.cameraHeight, this.cameraPosition.y);
        this.camera.setTarget(new Vector3(this.cameraPosition.x, 0, this.cameraPosition.y));

        //console.log(this.camera.position);
        this.ready = true;
    }

    resize() {
        this.engine.resize();
    }
}
