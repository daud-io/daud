import { Camera, Color3, DirectionalLight, Engine, FreeCamera, Matrix, MeshBuilder, PointLight, Scene, SceneLoader, StandardMaterial, Texture, Vector2, Vector3 } from "@babylonjs/core";
import { getTextureDefinition, load } from "./loader";
import "@babylonjs/loaders/OBJ";
import { OBJFileLoader } from "@babylonjs/loaders";

export class CustomContainer {
    scene: Scene;
    engine: Engine;
    camera: FreeCamera;
    light: DirectionalLight;
    pointLight: PointLight;
    //pointLight2: PointLight;

    constructor(canvas: HTMLCanvasElement) {
        this.engine = new Engine(canvas, true);
        this.scene = new Scene(this.engine);
        this.camera = new FreeCamera("Camera", new Vector3(0, 1000, 0), this.scene);
        this.light = new DirectionalLight("Light", new Vector3(0, -400, 0), this.scene);
        this.pointLight = new PointLight("Light", new Vector3(0, -4000, 0), this.scene);
        //this.pointLight2 = new PointLight("Light", new Vector3(0, -1000, 0), this.scene);

        this.defineGround();

        this.scene.preventDefaultOnPointerDown = false;
        this.scene.preventDefaultOnPointerUp = false;
    }

    defineGround() {
        const loadMesh = true;

        if (loadMesh) {
            SceneLoader.ShowLoadingScreen = false;
            OBJFileLoader.MATERIAL_LOADING_FAILS_SILENTLY = false;
            OBJFileLoader.OPTIMIZE_WITH_UV = true;

            SceneLoader.ImportMesh(null, "/assets/base/models/", "grid.glb", this.scene,
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


        /*
        const groundMaterial = new StandardMaterial("groundmaterial", this.scene);

        groundMaterial.diffuseTexture = new Texture("assets/themes/original/bg2.png", this.scene);
        groundMaterial.specularTexture = new Texture("assets/themes/original/bg2.png", this.scene);
        groundMaterial.emissiveTexture = new Texture("assets/themes/original/bg2.png", this.scene);
        groundMaterial.ambientTexture = new Texture("assets/themes/original/bg2.png", this.scene);

        const ground = MeshBuilder.CreateGround("Ground", { height: 30000, width: 30000, subdivisions: 4 });
        ground.position.y = -510;

        ground.material = groundMaterial;
        */
    }

    resize() {
        this.engine.resize();
    }

    toWorld(): Vector2 {
        var ray = this.scene.createPickingRay(this.scene.pointerX, this.scene.pointerY, Matrix.Identity(), this.camera);

        var hit = this.scene.pickWithRay(ray);
        return new Vector2(hit?.pickedPoint?.x ?? 0, hit?.pickedPoint?.z ?? 0);
    }
}
