import { Settings } from "../settings";
import { GameContainer } from "../gameContainer";
import { ClientGroup, ClientBody } from "../cache";
import { Ship } from "./ship";
import { DynamicTexture, Mesh, MeshBuilder, StandardMaterial, Vector2, Vector3 } from "@babylonjs/core";

export class Fleet {
    container: GameContainer;
    ID?: number;
    
    ships: { [id: number]: Ship };
    extraModes: string[];

    labelText: string = "";

    labelMesh: Mesh;
    textureWidth: number = 1024; 
    textureHeight: number = Settings.nameSize*2;
    textureLabel: DynamicTexture;
    materialLabel: StandardMaterial;

    fleetCenter: Vector2;

    constructor(container: GameContainer) {
        this.container = container;
        this.ID = undefined;
        this.ships = {};
        this.fleetCenter = Vector2.Zero();

        this.labelMesh = MeshBuilder.CreatePlane("fleet label", {
            width: this.textureWidth,
            height: this.textureHeight
        });

        this.labelMesh.rotate(new Vector3(1,0,0), Math.PI/2);
        this.labelMesh.isVisible = false;
        this.container.scene.addMesh(this.labelMesh);        

        this.textureLabel = new DynamicTexture("fleet label texture", {width: this.textureWidth, height: this.textureHeight}, this.container.scene, false);
        
        this.materialLabel = new StandardMaterial("fleet label material", this.container.scene);
        this.materialLabel.diffuseTexture = this.textureLabel;
        this.materialLabel.opacityTexture = this.textureLabel;
        this.materialLabel.diffuseTexture.level = 2;
        
        this.labelMesh.material = this.materialLabel;

        this.extraModes = [];
    }

    updateLabelText()
    {
        //Add text to dynamic texture
        var textureContext = this.textureLabel.getContext();
        textureContext.clearRect(0, 0, this.textureWidth, this.textureHeight);
        textureContext.font = `${Settings.nameSize}px sans-serif, system-ui`;

        var size = textureContext.measureText(this.labelText);
        textureContext.fillStyle = "#FFF";
        textureContext.fillText(this.labelText, this.textureWidth/2 - size.width/2, this.textureHeight/2 + size.actualBoundingBoxDescent);
        //this.textureLabel.drawText(this.labelText, 75, 135, font, "white", null!, true, true);
        this.textureLabel.update();
    }

    addShip(id: number, ship: Ship): void {
        this.ships[id] = ship;
    }
    deleteShip(id: number): void {
        delete this.ships[id];
    }
    update(groupUpdate: ClientGroup, myFleetID: number): void {
        if (this.labelText != groupUpdate.Caption)
        {
            this.labelText = groupUpdate.Caption ?? "";
            this.updateLabelText();
        }

        this.ID = groupUpdate.ID;
    }

    addPowerup(powerMode: string) {
        this.extraModes.push(powerMode);
        for (let id in this.ships) this.ships[id].updateTextureLayers();
    }
    removePowerup(powerMode: string) {
        this.extraModes = this.extraModes.filter((obj) => obj !== powerMode);
        for (let id in this.ships) this.ships[id].updateTextureLayers();
    }

    center(): Vector2 | null
    {
        let accX = 0,
        accY = 0,
        count = 0;

        for (const shipkey in this.ships) {
            const ship = this.ships[shipkey];
            accX += ship.body.Position.x;
            accY += ship.body.Position.y;
            count++;
        }

        if (Number.isNaN(accX) || Number.isNaN(accY))
            console.log('warn: NaN fleet center!');

        if (count > 0)
        {
            this.fleetCenter.x = accX / count;
            this.fleetCenter.y = accY / count;
            return this.fleetCenter;
        }
        else
            return null;

    }

    tick(time: number): void {
        //console.log(`Group: ${this.ID} ${this.caption} ${this.ships.length}`);

        const center = this.center();
        const offsetY = 0;
        if (center != null)
        {
            this.labelMesh.position.set(center.x, 200, center.y + offsetY);
            this.labelMesh.isVisible = true;
        }
        else
            this.labelMesh.isVisible = false;
    }

    dispose(): void {
        this.labelMesh.dispose();
        this.materialLabel.dispose();
        this.textureLabel.dispose();
    }
}

