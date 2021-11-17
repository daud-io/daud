import { GameContainer } from "./gameContainer";
import * as bus from "./bus";
import "@babylonjs/loaders/glTF";
import { AbstractMesh, Light, Node, PBRMaterial, SceneLoader, Vector2, Vector3 } from "@babylonjs/core";
import { GLTFFileLoader } from "@babylonjs/loaders";
import { Settings } from "./settings";

export class WorldMeshLoader {
    container: GameContainer;
    loadedFile: string | null;
    scalingVector: Vector3 = new Vector3(10, 10, 10);
    translationVector: Vector3 = new Vector3(0, 0, 0);

    constructor(container: GameContainer) {
        this.container = container;
        this.loadedFile = null;

        bus.on("hook", (hook) => this.onHook(hook));
        bus.on("disconnected", () => this.unloadMeshes());
    }

    onHook(hook: any) {
        console.log('meshloader new hook');
        if (!hook.Mesh)
            // old server/hook
            return;

        if (hook.Mesh.MeshURL != this.loadedFile) {
            if (this.loadedFile != null) this.unloadMeshes();

            if (hook.Mesh.Enabled !== null) {
                this.loadedFile = <string>(hook.Mesh.MeshURL);
                console.log('worldMeshLoader: loading mesh');

                if (hook.Mesh.Scale)
                {
                    this.scalingVector.x = hook.Mesh.Scale.X;
                    this.scalingVector.y = hook.Mesh.Scale.Y;
                    this.scalingVector.z = hook.Mesh.Scale.Z;
                }

                if (hook.Mesh.Translation)
                {
                    this.translationVector.x = hook.Mesh.Translation.X;
                    this.translationVector.y = hook.Mesh.Translation.Y;
                    this.translationVector.z = hook.Mesh.Translation.Z;
                }

                this.loadGLB(this.loadedFile);
            }
        }
        else
        {
            console.log(hook.Mesh);
            console.log(this.loadedFile);
            
            console.log('worldMeshLoader: meshless hook exit');
            this.container.ready = true;
        }

    }

    unloadMeshes() {
        var scene = this.container.scene;

        while (scene.meshes.length) scene.meshes[0].dispose();
        this.loadedFile = null;
        console.log('worldMeshLoader: unloading meshes');
        //this.container.scene.removeMesh(scene.meshes[0]);
    }

    recurseNodesAfterLoad(node: Node) {
        const extras = node.metadata?.gltf?.extras;

        if (extras != null) {
            if (extras.hidden == "true") {
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

        node.getChildren().forEach((child) => this.recurseNodesAfterLoad(child));
    }

    loadGLB(worldMesh: string) {
        SceneLoader.ShowLoadingScreen = false;
        //const plugin = <GLTFFileLoader>SceneLoader.Append("/api/v1/world/mesh/", worldMesh, this.container.scene);

        var url = worldMesh.indexOf('://') > -1
            ? worldMesh
            : `/api/v1/world/mesh/${worldMesh}`

        console.log({worldMesh, url});
        
        const plugin = <GLTFFileLoader>SceneLoader.Append(url, undefined, this.container.scene, () => {
            this.container.scene.rootNodes.forEach((rootNode) => {
                this.recurseNodesAfterLoad(rootNode);
            });


            const deleteLights:Light[] = [];

            this.container.scene.lights.forEach((light) => {
                if (light.name != "containerLight") {
                    deleteLights.push(light);
                    //light.intensity *= 10;
                    //light.shadowEnabled = true;
                    //var gen = new ShadowGenerator(1024, <IShadowLight>light);
                    //gen.addShadowCaster()
                }
            });

            for(let i in deleteLights)
                deleteLights[i].dispose();


            this.container.ready = true;            
        });

        plugin.onMeshLoadedObservable.add((mesh, state) => {
            if (mesh.name == "__root__") { // this node is added by the gltf importer to change handedness
                mesh.rotate(new Vector3(0, 1, 0), Math.PI); // rotate around Y axis because no one can agree on anything
                mesh.scaling = new Vector3(this.scalingVector.x * mesh.scaling.x, this.scalingVector.y * mesh.scaling.y, this.scalingVector.z * mesh.scaling.z);
                mesh.position = this.translationVector;
            }

            let pbr = mesh.material as PBRMaterial;
            if (pbr) {
                const extras = pbr?.metadata?.gltf?.extras;
                if (Settings.graphics == "high" && extras) {
                    if (extras.alpha) {
                        if (extras.alpha == "image") {
                            console.log(`Material change: ${pbr.name} to alpha from image`);
                            pbr.alphaMode = 1; // ALPHA_ADD
                            pbr.transparencyMode = 1; // ALPHA_TEST
                            pbr.albedoTexture.hasAlpha = true;
                        }

                        if (!isNaN(+extras.alpha)) {
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

