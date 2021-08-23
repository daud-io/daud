import { DirectionalLight, Engine, FreeCamera, HemisphericLight, Matrix, Mesh, MeshBuilder, PointLight, Scene, SceneLoader, StandardMaterial, Texture, Vector2, Vector3 } from "@babylonjs/core";
import "@babylonjs/loaders/glTF";
import { AdvancedDynamicTexture } from "@babylonjs/gui";

export class CustomContainer {
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
        //this.light = new HemisphericLight("HemiLight", new Vector3(1, 1, 1), this.scene);
        this.light = new DirectionalLight("DirectionalLight", new Vector3(1, -1, 1), this.scene);
        this.guiTexture = AdvancedDynamicTexture.CreateFullscreenUI("UI");
        //this.pointLight2 = new PointLight("Light", new Vector3(0, -1000, 0), this.scene);
        
        this.cameraPosition = Vector2.Zero();

        this.defineGround();
    }

    defineGround() {
        const loadMesh = true;

        if (loadMesh) {
            SceneLoader.ShowLoadingScreen = false;
            SceneLoader.ImportMesh(null, "/assets/base/models/", "ffa.glb", this.scene,
                function (abstractMesh) { // on success
                    // if you try to use the mesh scaling() vector directly, it doesn't something sort of like scaling,
                    // but a freaky sort of scaling that is not what we want.

                    // putting it inside a Mesh.CreateBox and scaling that makes all the difference.
                    
                    var m = Mesh.CreateBox('scaling', 15000, abstractMesh[0].getScene());
                    m.isVisible = false;
                    m.scaling = new Vector3(-10,10,10);
                    for (var i in abstractMesh)
                    {
                        const mesh = abstractMesh[i];
                        for (var j = 0; j < abstractMesh.length; j++)
                            mesh.parent = m;
                            
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
        this.cameraPosition.x = serverPosition.x * 0.2 + this.cameraPosition.x * 0.8;
        this.cameraPosition.y = serverPosition.y * 0.2 + this.cameraPosition.y * 0.8;
        this.camera.position = new Vector3(this.cameraPosition.x, this.cameraHeight, this.cameraPosition.y);
        this.camera.setTarget(new Vector3(this.cameraPosition.x, 0, this.cameraPosition.y));
        this.ready = true;
    }

    resize() {
        this.engine.resize();
    }
}
