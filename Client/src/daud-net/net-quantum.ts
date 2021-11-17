// automatically generated by the FlatBuffers compiler, do not modify

import * as flatbuffers from "flatbuffers";

import { AllMessages, unionToAllMessages, unionListToAllMessages } from "../daud-net/all-messages";

export class NetQuantum {
    bb: flatbuffers.ByteBuffer | null = null;
    bb_pos = 0;
    __init(i: number, bb: flatbuffers.ByteBuffer): NetQuantum {
        this.bb_pos = i;
        this.bb = bb;
        return this;
    }

    static getRootAsNetQuantum(bb: flatbuffers.ByteBuffer, obj?: NetQuantum): NetQuantum {
        return (obj || new NetQuantum()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
    }

    static getSizePrefixedRootAsNetQuantum(bb: flatbuffers.ByteBuffer, obj?: NetQuantum): NetQuantum {
        bb.setPosition(bb.position() + flatbuffers.SIZE_PREFIX_LENGTH);
        return (obj || new NetQuantum()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
    }

    messageType(): AllMessages {
        const offset = this.bb!.__offset(this.bb_pos, 4);
        return offset ? this.bb!.readUint8(this.bb_pos + offset) : AllMessages.NONE;
    }

    message<T extends flatbuffers.Table>(obj: any): any | null {
        const offset = this.bb!.__offset(this.bb_pos, 6);
        return offset ? this.bb!.__union(obj, this.bb_pos + offset) : null;
    }

    static startNetQuantum(builder: flatbuffers.Builder) {
        builder.startObject(2);
    }

    static addMessageType(builder: flatbuffers.Builder, messageType: AllMessages) {
        builder.addFieldInt8(0, messageType, AllMessages.NONE);
    }

    static addMessage(builder: flatbuffers.Builder, messageOffset: flatbuffers.Offset) {
        builder.addFieldOffset(1, messageOffset, 0);
    }

    static endNetQuantum(builder: flatbuffers.Builder): flatbuffers.Offset {
        const offset = builder.endObject();
        return offset;
    }

    static finishNetQuantumBuffer(builder: flatbuffers.Builder, offset: flatbuffers.Offset) {
        builder.finish(offset);
    }

    static finishSizePrefixedNetQuantumBuffer(builder: flatbuffers.Builder, offset: flatbuffers.Offset) {
        builder.finish(offset, undefined, true);
    }

    static createNetQuantum(builder: flatbuffers.Builder, messageType: AllMessages, messageOffset: flatbuffers.Offset): flatbuffers.Offset {
        NetQuantum.startNetQuantum(builder);
        NetQuantum.addMessageType(builder, messageType);
        NetQuantum.addMessage(builder, messageOffset);
        return NetQuantum.endNetQuantum(builder);
    }
}