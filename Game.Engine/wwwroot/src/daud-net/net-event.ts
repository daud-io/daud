// automatically generated by the FlatBuffers compiler, do not modify

import * as flatbuffers from "flatbuffers";

export class NetEvent {
    bb: flatbuffers.ByteBuffer | null = null;
    bb_pos = 0;
    __init(i: number, bb: flatbuffers.ByteBuffer): NetEvent {
        this.bb_pos = i;
        this.bb = bb;
        return this;
    }

    static getRootAsNetEvent(bb: flatbuffers.ByteBuffer, obj?: NetEvent): NetEvent {
        return (obj || new NetEvent()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
    }

    static getSizePrefixedRootAsNetEvent(bb: flatbuffers.ByteBuffer, obj?: NetEvent): NetEvent {
        bb.setPosition(bb.position() + flatbuffers.SIZE_PREFIX_LENGTH);
        return (obj || new NetEvent()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
    }

    type(): string | null;
    type(optionalEncoding: flatbuffers.Encoding): string | Uint8Array | null;
    type(optionalEncoding?: any): string | Uint8Array | null {
        const offset = this.bb!.__offset(this.bb_pos, 4);
        return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
    }

    data(): string | null;
    data(optionalEncoding: flatbuffers.Encoding): string | Uint8Array | null;
    data(optionalEncoding?: any): string | Uint8Array | null {
        const offset = this.bb!.__offset(this.bb_pos, 6);
        return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
    }

    static startNetEvent(builder: flatbuffers.Builder) {
        builder.startObject(2);
    }

    static addType(builder: flatbuffers.Builder, typeOffset: flatbuffers.Offset) {
        builder.addFieldOffset(0, typeOffset, 0);
    }

    static addData(builder: flatbuffers.Builder, dataOffset: flatbuffers.Offset) {
        builder.addFieldOffset(1, dataOffset, 0);
    }

    static endNetEvent(builder: flatbuffers.Builder): flatbuffers.Offset {
        const offset = builder.endObject();
        return offset;
    }

    static createNetEvent(builder: flatbuffers.Builder, typeOffset: flatbuffers.Offset, dataOffset: flatbuffers.Offset): flatbuffers.Offset {
        NetEvent.startNetEvent(builder);
        NetEvent.addType(builder, typeOffset);
        NetEvent.addData(builder, dataOffset);
        return NetEvent.endNetEvent(builder);
    }
}
