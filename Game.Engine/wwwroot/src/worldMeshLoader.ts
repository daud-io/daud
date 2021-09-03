import { GameContainer } from './gameContainer';
import bus from "./bus";
import "@babylonjs/loaders/glTF";
import { Color3, CubeTexture, Mesh, MeshBuilder, SceneLoader, TransformNode, Vector3, VertexBuffer } from "@babylonjs/core";
import { GLTFFileLoader } from '@babylonjs/loaders';
import { instancesDeclaration } from '@babylonjs/core/Shaders/ShadersInclude/instancesDeclaration';

export class WorldMeshLoader {
    container: GameContainer;
    loadedFile: string | null;

    constructor(container:GameContainer) {
        this.container = container;
        this.loadedFile = null;

        bus.on("hook", (hook) => this.onHook(hook));
    }

    onHook(hook:any)
    {
        if (!hook.Mesh) // old server/hook
            return;

        if (hook.Mesh.MeshURL != this.loadedFile)
        {
            if (this.loadedFile != null)
                this.unloadMeshes();

            if (hook.Mesh.Enabled != null)
            {
                this.loadedFile = <string>hook.Mesh.MeshURL;
                this.loadGLB(this.loadedFile);
            }
        }
    }

    unloadMeshes()
    {
        var scene = this.container.scene;
        while(scene.meshes.length)
            scene.meshes[0].dispose();
            //this.container.scene.removeMesh(scene.meshes[0]);
    }

    loadGLB(worldMesh: string)
    {
        SceneLoader.ShowLoadingScreen = false;
        //const plugin = <GLTFFileLoader>SceneLoader.Append("/api/v1/world/mesh/", worldMesh, this.container.scene);
        const plugin = <GLTFFileLoader>SceneLoader.Append("/api/v1/world/mesh/", worldMesh, this.container.scene, () => {
            this.container.scene.lights.forEach(light => {
                //light.intensity *= 10;
            });

        });

        plugin.onMeshLoadedObservable.add((mesh, state) => {
            if (mesh.name == "__root__")
            {
                mesh.scaling = new Vector3(10 * mesh.scaling.x, 10 * mesh.scaling.y, 10 * mesh.scaling.z);
                mesh.rotate(new Vector3(0, 1, 0), Math.PI);
            }
        });

        plugin.onParsedObservable.add(() => {

            var l = this.container.scene.lights;

            
        });
        
    }
}