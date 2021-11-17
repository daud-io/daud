// automatically generated by the FlatBuffers compiler, do not modify

import * as flatbuffers from "flatbuffers";

export class NetControlInput {
    bb: flatbuffers.ByteBuffer | null = null;
    bb_pos = 0;
    __init(i: number, bb: flatbuffers.ByteBuffer): NetControlInput {
        this.bb_pos = i;
        this.bb = bb;
        return this;
    }

    static getRootAsNetControlInput(bb: flatbuffers.ByteBuffer, obj?: NetControlInput): NetControlInput {
        return (obj || new NetControlInput()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
    }

    static getSizePrefixedRootAsNetControlInput(bb: flatbuffers.ByteBuffer, obj?: NetControlInput): NetControlInput {
        bb.setPosition(bb.position() + flatbuffers.SIZE_PREFIX_LENGTH);
        return (obj || new NetControlInput()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
    }

    angle(): number {
        const offset = this.bb!.__offset(this.bb_pos, 4);
        return offset ? this.bb!.readFloat32(this.bb_pos + offset) : 0.0;
    }

    x(): number {
        const offset = this.bb!.__offset(this.bb_pos, 6);
        return offset ? this.bb!.readFloat32(this.bb_pos + offset) : 0.0;
    }

    y(): number {
        const offset = this.bb!.__offset(this.bb_pos, 8);
        return offset ? this.bb!.readFloat32(this.bb_pos + offset) : 0.0;
    }

    boost(): boolean {
        const offset = this.bb!.__offset(this.bb_pos, 10);
        return offset ? !!this.bb!.readInt8(this.bb_pos + offset) : false;
    }

    shoot(): boolean {
        const offset = this.bb!.__offset(this.bb_pos, 12);
        return offset ? !!this.bb!.readInt8(this.bb_pos + offset) : false;
    }

    spectatecontrol(): string | null;
    spectatecontrol(optionalEncoding: flatbuffers.Encoding): string | Uint8Array | null;
    spectatecontrol(optionalEncoding?: any): string | Uint8Array | null {
        const offset = this.bb!.__offset(this.bb_pos, 14);
        return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
    }

    customdata(): string | null;
    customdata(optionalEncoding: flatbuffers.Encoding): string | Uint8Array | null;
    customdata(optionalEncoding?: any): string | Uint8Array | null {
        const offset = this.bb!.__offset(this.bb_pos, 16);
        return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
    }

    static startNetControlInput(builder: flatbuffers.Builder) {
        builder.startObject(7);
    }

    static addAngle(builder: flatbuffers.Builder, angle: number) {
        builder.addFieldFloat32(0, angle, 0.0);
    }

    static addX(builder: flatbuffers.Builder, x: number) {
        builder.addFieldFloat32(1, x, 0.0);
    }

    static addY(builder: flatbuffers.Builder, y: number) {
        builder.addFieldFloat32(2, y, 0.0);
    }

    static addBoost(builder: flatbuffers.Builder, boost: boolean) {
        builder.addFieldInt8(3, +boost, +false);
    }

    static addShoot(builder: flatbuffers.Builder, shoot: boolean) {
        builder.addFieldInt8(4, +shoot, +false);
    }

    static addSpectatecontrol(builder: flatbuffers.Builder, spectatecontrolOffset: flatbuffers.Offset) {
        builder.addFieldOffset(5, spectatecontrolOffset, 0);
    }

    static addCustomdata(builder: flatbuffers.Builder, customdataOffset: flatbuffers.Offset) {
        builder.addFieldOffset(6, customdataOffset, 0);
    }

    static endNetControlInput(builder: flatbuffers.Builder): flatbuffers.Offset {
        const offset = builder.endObject();
        return offset;
    }

    static createNetControlInput(
        builder: flatbuffers.Builder,
        angle: number,
        x: number,
        y: number,
        boost: boolean,
        shoot: boolean,
        spectatecontrolOffset: flatbuffers.Offset,
        customdataOffset: flatbuffers.Offset
    ): flatbuffers.Offset {
        NetControlInput.startNetControlInput(builder);
        NetControlInput.addAngle(builder, angle);
        NetControlInput.addX(builder, x);
        NetControlInput.addY(builder, y);
        NetControlInput.addBoost(builder, boost);
        NetControlInput.addShoot(builder, shoot);
        NetControlInput.addSpectatecontrol(builder, spectatecontrolOffset);
        NetControlInput.addCustomdata(builder, customdataOffset);
        return NetControlInput.endNetControlInput(builder);
    }
}