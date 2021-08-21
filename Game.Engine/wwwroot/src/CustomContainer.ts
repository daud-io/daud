import { DirectionalLight, Engine, FreeCamera, Matrix, MeshBuilder, PointLight, Scene, SceneLoader, StandardMaterial, Texture, Vector2, Vector3 } from "@babylonjs/core";
import "@babylonjs/loaders/glTF";
import { AdvancedDynamicTexture } from "@babylonjs/gui";

export class CustomContainer {
    scene: Scene;
    engine: Engine;
    camera: FreeCamera;
    light: DirectionalLight;
    pointLight: PointLight;
    //pointLight2: PointLight;

    guiTexture: AdvancedDynamicTexture;

    constructor(canvas: HTMLCanvasElement) {
        this.engine = new Engine(canvas, true);
        this.scene = new Scene(this.engine);
        this.camera = new FreeCamera("Camera", new Vector3(0, 1000, 0), this.scene);
        this.light = new DirectionalLight("Light", new Vector3(0, -400, 0), this.scene);
        this.pointLight = new PointLight("Light", new Vector3(0, -4000, 0), this.scene);
        this.guiTexture = AdvancedDynamicTexture.CreateFullscreenUI("UI");
        //this.pointLight2 = new PointLight("Light", new Vector3(0, -1000, 0), this.scene);

        this.defineGround();

        this.scene.preventDefaultOnPointerDown = false;
        this.scene.preventDefaultOnPointerUp = false;
        
    }

    defineGround() {
        const loadMesh = true;

        if (loadMesh) {
            SceneLoader.ShowLoadingScreen = false;
            SceneLoader.ImportMesh(null, "/assets/base/models/", "partycity.glb", this.scene,
                function (scene) { // on success
                    let x = 1;
                    for(let i in scene)
                    {
                        
                    }

                },
                () => {}, // progress
                (scene, errorMessage, exception) => { // on error
                    let x = 1;
                }
            );
        }

        const groundMaterial = new StandardMaterial("groundmaterial", this.scene);

        groundMaterial.diffuseTexture = new Texture("assets/themes/original/bg2.png", this.scene);
        groundMaterial.specularTexture = new Texture("assets/themes/original/bg2.png", this.scene);
        groundMaterial.emissiveTexture = new Texture("assets/themes/original/bg2.png", this.scene);
        groundMaterial.ambientTexture = new Texture("assets/themes/original/bg2.png", this.scene);

        const ground = MeshBuilder.CreateGround("Ground", { height: 30000, width: 30000, subdivisions: 4 });
        ground.position.y = -510;
        ground.enablePointerMoveEvents = true;

        ground.material = groundMaterial;
    }

    resize() {
        this.engine.resize();
    }
}
