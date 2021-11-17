// automatically generated by the FlatBuffers compiler, do not modify

import * as flatbuffers from "flatbuffers";

export class NetSpawn {
    bb: flatbuffers.ByteBuffer | null = null;
    bb_pos = 0;
    __init(i: number, bb: flatbuffers.ByteBuffer): NetSpawn {
        this.bb_pos = i;
        this.bb = bb;
        return this;
    }

    static getRootAsNetSpawn(bb: flatbuffers.ByteBuffer, obj?: NetSpawn): NetSpawn {
        return (obj || new NetSpawn()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
    }

    static getSizePrefixedRootAsNetSpawn(bb: flatbuffers.ByteBuffer, obj?: NetSpawn): NetSpawn {
        bb.setPosition(bb.position() + flatbuffers.SIZE_PREFIX_LENGTH);
        return (obj || new NetSpawn()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
    }

    name(): string | null;
    name(optionalEncoding: flatbuffers.Encoding): string | Uint8Array | null;
    name(optionalEncoding?: any): string | Uint8Array | null {
        const offset = this.bb!.__offset(this.bb_pos, 4);
        return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
    }

    ship(): string | null;
    ship(optionalEncoding: flatbuffers.Encoding): string | Uint8Array | null;
    ship(optionalEncoding?: any): string | Uint8Array | null {
        const offset = this.bb!.__offset(this.bb_pos, 6);
        return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
    }

    color(): string | null;
    color(optionalEncoding: flatbuffers.Encoding): string | Uint8Array | null;
    color(optionalEncoding?: any): string | Uint8Array | null {
        const offset = this.bb!.__offset(this.bb_pos, 8);
        return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
    }

    token(): string | null;
    token(optionalEncoding: flatbuffers.Encoding): string | Uint8Array | null;
    token(optionalEncoding?: any): string | Uint8Array | null {
        const offset = this.bb!.__offset(this.bb_pos, 10);
        return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
    }

    static startNetSpawn(builder: flatbuffers.Builder) {
        builder.startObject(4);
    }

    static addName(builder: flatbuffers.Builder, nameOffset: flatbuffers.Offset) {
        builder.addFieldOffset(0, nameOffset, 0);
    }

    static addShip(builder: flatbuffers.Builder, shipOffset: flatbuffers.Offset) {
        builder.addFieldOffset(1, shipOffset, 0);
    }

    static addColor(builder: flatbuffers.Builder, colorOffset: flatbuffers.Offset) {
        builder.addFieldOffset(2, colorOffset, 0);
    }

    static addToken(builder: flatbuffers.Builder, tokenOffset: flatbuffers.Offset) {
        builder.addFieldOffset(3, tokenOffset, 0);
    }

    static endNetSpawn(builder: flatbuffers.Builder): flatbuffers.Offset {
        const offset = builder.endObject();
        return offset;
    }

    static createNetSpawn(
        builder: flatbuffers.Builder,
        nameOffset: flatbuffers.Offset,
        shipOffset: flatbuffers.Offset,
        colorOffset: flatbuffers.Offset,
        tokenOffset: flatbuffers.Offset
    ): flatbuffers.Offset {
        NetSpawn.startNetSpawn(builder);
        NetSpawn.addName(builder, nameOffset);
        NetSpawn.addShip(builder, shipOffset);
        NetSpawn.addColor(builder, colorOffset);
        NetSpawn.addToken(builder, tokenOffset);
        return NetSpawn.endNetSpawn(builder);
    }
}