import { RenderedObject } from "../renderedObject";
import { Fleet } from "./fleet";
import { CustomContainer } from "../CustomContainer";
import { ClientBody, ClientGroup, getGroup } from "../cache";

export type TokenData ={
    FleetID: number | undefined;
}
export class Token extends RenderedObject {
    fleet: Fleet | undefined;
    bodyID: string;
    group: ClientGroup;
    tokenData: TokenData;

    constructor(container: CustomContainer, clientBody: ClientBody, group: ClientGroup) {
        super(container, clientBody);
        this.fleet = undefined;
        this.bodyID = `b-${clientBody.ID}`;
        this.group = group;        
        this.tokenData = this.defaultTokenData();
    }

    decodeOrderedModes(mode: number) {
        var modes: string[] = [];

        if (mode == 0) modes.push("default");
        if ((mode & 1) != 0) modes.push("carried");
        if ((mode & 2) != 0) modes.push("expiring");

        return modes;
    }

    defaultTokenData()
    {
        return { FleetID: undefined };
    }

    updateGroupData()
    {
        if (this.group.CustomData != this.tokenData)
        {
            if (this.group.CustomData)
                this.tokenData = this.group.CustomData as TokenData;
            else
                this.tokenData = this.defaultTokenData();
        }
    }

    destroy() {
        //if (this.fleet) 
            //this.fleet.deleteShip(this.bodyID);

        super.destroy();
    }

    update() {
        
        super.update();
        this.updateGroupData();
    
        if (this.tokenData.FleetID != this.fleet?.ID)
        {
            if (this.tokenData.FleetID)
            {
                let group = getGroup(this.tokenData.FleetID);
                this.fleet = group?.renderer;
                this.fleet?.addPowerup("haste");
            }
            else
            {
                if (this.fleet)
                    this.fleet.removePowerup("haste");
                    
                this.fleet = undefined;
            }
        }

        // when a ship is abandoned, the ship lives on
        // but it's disconnected from its group
        /*if (this.fleet && this.body.Group != this.fleet.ID) {
            this.fleet.deleteShip(this.bodyID);
            this.fleet = null;
        }*/
    }
}
