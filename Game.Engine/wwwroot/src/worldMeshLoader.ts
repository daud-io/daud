import { GameContainer } from './gameContainer';
import bus from "./bus";
import "@babylonjs/loaders/glTF";
import { AbstractMesh, Node, PBRMaterial, SceneLoader, Vector3 } from "@babylonjs/core";
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

    recurseNodesAfterLoad(node: Node)
    {
        const extras = node.metadata?.gltf?.extras;

        if (extras != null)
        {
            if (extras.hidden == "true")
            {
                console.log(`deleting hidden node: ${node.name}`);
                node.dispose();
                return;
            }

            /*if (extras.alpha != null)
            {
                console.log(`setting alpha on node ${node.name}`);

                const mesh = node as AbstractMesh;
                if (mesh)
                {
                    console.log(`setting alpha on mesh ${mesh.name}`);
                    if (mesh.material)
                    {
                        console.log(`setting alpha on material ${mesh.material.name} to ${extras.alpha}`);
                        mesh.material.alpha = Number(extras.alpha);
                        mesh.material.transparencyMode = 2; //ALPHA_BLEND
                        mesh.material.alphaMode = 2; // ALPHA_COMBINE
                    }
                };
            }
            */
        }

        node.getChildren().forEach(child => this.recurseNodesAfterLoad(child));
    }
    
    loadGLB(worldMesh: string)
    {
        SceneLoader.ShowLoadingScreen = false;
        //const plugin = <GLTFFileLoader>SceneLoader.Append("/api/v1/world/mesh/", worldMesh, this.container.scene);
        const plugin = <GLTFFileLoader>SceneLoader.Append("/api/v1/world/mesh/", worldMesh, this.container.scene, () => {

            this.container.scene.rootNodes.forEach(rootNode => {
                this.recurseNodesAfterLoad(rootNode);
            });

            this.container.scene.lights.forEach(light => {
                if (light.name != "containerLight")
                {
                    light.dispose();
                    //light.intensity *= 10;
                    //light.shadowEnabled = true;
                    //var gen = new ShadowGenerator(1024, <IShadowLight>light);
                    //gen.addShadowCaster()
                }
            });
        });

        plugin.onMeshLoadedObservable.add((mesh, state) => {
            if (mesh.name == "__root__")
            {
                mesh.scaling = new Vector3(10 * mesh.scaling.x, 10 * mesh.scaling.y, 10 * mesh.scaling.z);
                mesh.rotate(new Vector3(0, 1, 0), Math.PI);
            }


            let pbr = mesh.material as PBRMaterial;
            if (pbr)
            {
                const extras = pbr?.metadata?.gltf?.extras;
                if (extras)
                {
                    if (extras.alpha)
                    {
                        if (extras.alpha == "image")
                        {
                            console.log(`Material change: ${pbr.name} to alpha from image`);
                            pbr.alphaMode = 1; // ALPHA_ADD
                            pbr.transparencyMode = 1 // ALPHA_TEST
                            pbr.albedoTexture.hasAlpha = true;
                        }
                    
                        if (!isNaN(+extras.alpha))
                        {
                            console.log(`Material change: ${pbr.name} alpha:${extras.alpha}`);
                            pbr.alpha = Number(extras.alpha);
                            pbr.transparencyMode = 2; //ALPHA_BLEND
                            pbr.alphaMode = 2; // ALPHA_COMBINE
                        }
                    }

                }
                //pbr.maxSimultaneousLights = 12;
            }

            //mesh.receiveShadows = true;
            //this.container.shadowGenerator.addShadowCaster(mesh);
        });

        plugin.onParsedObservable.add(() => {
            var l = this.container.scene.lights;

            
        });
        
    }
}