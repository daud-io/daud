import { GameContainer } from './gameContainer';
import bus from "./bus";
import "@babylonjs/loaders/glTF";
import { Mesh, SceneLoader, Vector3 } from "@babylonjs/core";
import { GLTFFileLoader } from '@babylonjs/loaders';

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
        SceneLoader.ImportMesh(null, "/api/v1/world/mesh/", worldMesh, this.container.scene,
            function (abstractMesh) { // on success
                // if you try to use the mesh scaling() vector directly, it does something sort of like scaling,
                // but a freaky sort of scaling that is not what we want.

                // putting it inside a Mesh.CreateBox and scaling that makes all the difference.
                
                var m = Mesh.CreateBox('scaling', 1, abstractMesh[0].getScene());
                m.isVisible = false;
                m.scaling = new Vector3(-10,10,10);
                for (var i in abstractMesh)
                {
                    const mesh = abstractMesh[i];
                    for (var j = 0; j < abstractMesh.length; j++)
                        mesh.parent = m;
                        
                    // property-based material changes
                    /*if (mesh.material && mesh.material.name.indexOf('invisible.png') > -1)
                        mesh.dispose();*/
                }
            },
            () => {}, // progress
            (scene, errorMessage, exception) => { // on error
                console.log(`error in WorldMeshLoader: ${errorMessage}`);
            }
        );
    }
}