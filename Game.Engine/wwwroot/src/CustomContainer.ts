import { DirectionalLight, Engine, FreeCamera, HemisphericLight, Matrix, Mesh, MeshBuilder, PointLight, Scene, SceneLoader, StandardMaterial, Texture, Vector2, Vector3 } from "@babylonjs/core";
import "@babylonjs/loaders/glTF";
import { AdvancedDynamicTexture } from "@babylonjs/gui";

export class CustomContainer {
    scene: Scene;
    engine: Engine;
    camera: FreeCamera;
    light: HemisphericLight;

    guiTexture: AdvancedDynamicTexture;
    lastCamera: Vector2;
    lastPosition: Vector2;
    cameraHeight: number;

    ready: boolean;

    constructor(canvas: HTMLCanvasElement) {
        this.ready = false;
        this.engine = new Engine(canvas, true);
        this.scene = new Scene(this.engine);
        this.camera = new FreeCamera("Camera", new Vector3(0, 1000, 0), this.scene);
        this.light = new HemisphericLight("HemiLight", new Vector3(1, 1, 1), this.scene);
        this.guiTexture = AdvancedDynamicTexture.CreateFullscreenUI("UI");
        this.cameraHeight = 4000;
        //this.pointLight2 = new PointLight("Light", new Vector3(0, -1000, 0), this.scene);
        
        this.lastCamera = this.lastPosition = Vector2.Zero();

        this.defineGround();
    }

    defineGround() {
        const loadMesh = true;

        if (loadMesh) {
            SceneLoader.ShowLoadingScreen = false;
            SceneLoader.ImportMesh(null, "/assets/base/models/", "ffa.glb", this.scene,
                function (abstractMesh) { // on success

                    var m = Mesh.CreateBox('scaling', 15000, abstractMesh[0].getScene());
                    m.isVisible = false;
                    m.scaling = new Vector3(-10,10,10);
                    for (var i in abstractMesh)
                    {
                        const mesh = abstractMesh[i];
                        for (var j = 0; j < abstractMesh.length; j++)
                            abstractMesh[i].parent = m;
                            
                        /*if (mesh.material && mesh.material.name.indexOf('invisible.png') > -1)
                            mesh.dispose();*/
                        
                    }
                },
                () => {}, // progress
                (scene, errorMessage, exception) => { // on error
                    let x = 1;
                }
            );
        }

        /*const groundMaterial = new StandardMaterial("groundmaterial", this.scene);

        groundMaterial.diffuseTexture = new Texture("assets/themes/original/bg2.png", this.scene);
        groundMaterial.specularTexture = new Texture("assets/themes/original/bg2.png", this.scene);
        groundMaterial.emissiveTexture = new Texture("assets/themes/original/bg2.png", this.scene);
        groundMaterial.ambientTexture = new Texture("assets/themes/original/bg2.png", this.scene);

        const ground = MeshBuilder.CreateGround("Ground", { height: 30000, width: 30000, subdivisions: 4 });
        ground.position.y = -510;
        //ground.enablePointerMoveEvents = true;

        ground.material = groundMaterial;*/
    }

    PositionCamera(serverPosition: Vector2)
    {
        const position = new Vector2(0, 0);
        position.x = serverPosition.x * 0.2 + this.lastCamera.x * 0.8;
        position.y = serverPosition.y * 0.2 + this.lastCamera.y * 0.8;
        this.lastCamera = position;
        this.camera.position = new Vector3(position.x, this.cameraHeight, position.y);
        this.camera.setTarget(new Vector3(position.x, 0, position.y));
        this.lastPosition = position;
        this.ready = true;
    }

    resize() {
        this.engine.resize();
    }
}
