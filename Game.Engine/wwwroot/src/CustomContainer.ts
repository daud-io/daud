import { Camera, Color3, DirectionalLight, Engine, FreeCamera, Matrix, MeshBuilder, PointLight, Scene, StandardMaterial, Vector2, Vector3 } from "@babylonjs/core";

export class CustomContainer {
    scene: Scene;
    engine: Engine;
    camera: FreeCamera;
    light: DirectionalLight;

    constructor(canvas: HTMLCanvasElement)
    {
        this.engine = new Engine(canvas, true);
        this.scene = new Scene(this.engine);
        this.camera = new FreeCamera("Camera", new Vector3(0, 1000, 0), this.scene);
        this.light = new DirectionalLight("Light", new Vector3(0, 1, 0), this.scene);

        this.defineGround();
        
        this.scene.preventDefaultOnPointerDown = false;
        this.scene.preventDefaultOnPointerUp = false;
    }

    defineGround()
    {
        const groundMaterial = new StandardMaterial("groundmaterial", this.scene);
        
        groundMaterial.diffuseColor = new Color3(1, 1, 1);
        groundMaterial.specularColor = new Color3(0.5, 0.6, 0.87);
        groundMaterial.emissiveColor = new Color3(0.2, 0.2, 0.2);
        groundMaterial.ambientColor = new Color3(0.23, 0.98, 0.53);

        const ground = MeshBuilder.CreateGround("Ground", {height: 30000, width: 30000, subdivisions: 4});
        ground.position.y = -10;
        
        ground.material = groundMaterial;
    }

    resize()
    {
        this.engine.resize();
    }

    toWorld(): Vector2 {
        var ray = this.scene.createPickingRay(this.scene.pointerX, this.scene.pointerY, Matrix.Identity(), this.camera);	

        var hit = this.scene.pickWithRay(ray);
        return new Vector2(hit?.pickedPoint?.x ?? 0, hit?.pickedPoint?.z ?? 0);
    }
}
