import { CubeTexture, DirectionalLight, Engine, FreeCamera, HemisphericLight, Matrix, Mesh, MeshBuilder, PointLight, Quaternion, Scene, SceneLoader, ShadowGenerator, StandardMaterial, Texture, Vector2, Vector3 } from "@babylonjs/core";
import { AdvancedDynamicTexture } from "@babylonjs/gui";

export class GameContainer {
    scene: Scene;
    engine: Engine;
    camera: FreeCamera;

    guiTexture: AdvancedDynamicTexture;
    cameraPosition: Vector2;
    cameraHeight: number;

    ready: boolean;
    //shadowGenerator: ShadowGenerator;
    //light: DirectionalLight;

    constructor(canvas: HTMLCanvasElement) {
        this.ready = false;
        this.engine = new Engine(canvas, true);
        this.scene = new Scene(this.engine);
        this.cameraHeight = 4000;
        //this.cameraHeight = 6000;
        this.camera = new FreeCamera("Camera", new Vector3(0, this.cameraHeight, 0), this.scene);
        this.camera.setTarget(new Vector3(0,0,0));
        this.camera.maxZ = 20000;

        var light = new HemisphericLight("containerLight", new Vector3(0, 1, 0), this.scene);
        light.intensity *= 0.3;
        //var light = new DirectionalLight("containerLight", new Vector3(0, -1, 0), this.scene);
        //light.position = new Vector3(0, 500, 0);
        //light.intensity = 50;

        //this.light.shadowEnabled = false;
        //this.light.shadowMaxZ = 10000;
        //this.light.shadowMinZ = -10000;
        //this.light.intensity = 0;
        
        //this.shadowGenerator = new ShadowGenerator(1024, this.light);
        //this.shadowGenerator.bias = 0.01;


        this.guiTexture = AdvancedDynamicTexture.CreateFullscreenUI("UI");
        
        this.cameraPosition = Vector2.Zero();
        //this.scene.environmentTexture = CubeTexture.CreateFromPrefilteredData("/assets/base/models/environment.env", this.scene);
        

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
        this.camera.upVector = new Vector3(0, 0, 1);
        this.camera.setTarget(new Vector3(this.cameraPosition.x, 0, this.cameraPosition.y));

        //console.log(this.camera.position);
        this.ready = true;
    }

    resize() {
        this.engine.resize();
    }
}
