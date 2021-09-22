// automatically generated by the FlatBuffers compiler, do not modify

import * as flatbuffers from "flatbuffers";

import { Vec2 } from "../daud-net/vec2";

export class NetLeaderboardEntry {
    bb: flatbuffers.ByteBuffer | null = null;
    bb_pos = 0;
    __init(i: number, bb: flatbuffers.ByteBuffer): NetLeaderboardEntry {
        this.bb_pos = i;
        this.bb = bb;
        return this;
    }

    static getRootAsNetLeaderboardEntry(bb: flatbuffers.ByteBuffer, obj?: NetLeaderboardEntry): NetLeaderboardEntry {
        return (obj || new NetLeaderboardEntry()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
    }

    static getSizePrefixedRootAsNetLeaderboardEntry(bb: flatbuffers.ByteBuffer, obj?: NetLeaderboardEntry): NetLeaderboardEntry {
        bb.setPosition(bb.position() + flatbuffers.SIZE_PREFIX_LENGTH);
        return (obj || new NetLeaderboardEntry()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
    }

    fleetid(): number {
        const offset = this.bb!.__offset(this.bb_pos, 4);
        return offset ? this.bb!.readUint32(this.bb_pos + offset) : 0;
    }

    name(): string | null;
    name(optionalEncoding: flatbuffers.Encoding): string | Uint8Array | null;
    name(optionalEncoding?: any): string | Uint8Array | null {
        const offset = this.bb!.__offset(this.bb_pos, 6);
        return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
    }

    score(): number {
        const offset = this.bb!.__offset(this.bb_pos, 8);
        return offset ? this.bb!.readInt32(this.bb_pos + offset) : 0;
    }

    color(): string | null;
    color(optionalEncoding: flatbuffers.Encoding): string | Uint8Array | null;
    color(optionalEncoding?: any): string | Uint8Array | null {
        const offset = this.bb!.__offset(this.bb_pos, 10);
        return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
    }

    position(obj?: Vec2): Vec2 | null {
        const offset = this.bb!.__offset(this.bb_pos, 12);
        return offset ? (obj || new Vec2()).__init(this.bb_pos + offset, this.bb!) : null;
    }

    token(): boolean {
        const offset = this.bb!.__offset(this.bb_pos, 14);
        return offset ? !!this.bb!.readInt8(this.bb_pos + offset) : false;
    }

    modedata(): string | null;
    modedata(optionalEncoding: flatbuffers.Encoding): string | Uint8Array | null;
    modedata(optionalEncoding?: any): string | Uint8Array | null {
        const offset = this.bb!.__offset(this.bb_pos, 16);
        return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
    }

    static startNetLeaderboardEntry(builder: flatbuffers.Builder) {
        builder.startObject(7);
    }

    static addFleetid(builder: flatbuffers.Builder, fleetid: number) {
        builder.addFieldInt32(0, fleetid, 0);
    }

    static addName(builder: flatbuffers.Builder, nameOffset: flatbuffers.Offset) {
        builder.addFieldOffset(1, nameOffset, 0);
    }

    static addScore(builder: flatbuffers.Builder, score: number) {
        builder.addFieldInt32(2, score, 0);
    }

    static addColor(builder: flatbuffers.Builder, colorOffset: flatbuffers.Offset) {
        builder.addFieldOffset(3, colorOffset, 0);
    }

    static addPosition(builder: flatbuffers.Builder, positionOffset: flatbuffers.Offset) {
        builder.addFieldStruct(4, positionOffset, 0);
    }

    static addToken(builder: flatbuffers.Builder, token: boolean) {
        builder.addFieldInt8(5, +token, +false);
    }

    static addModedata(builder: flatbuffers.Builder, modedataOffset: flatbuffers.Offset) {
        builder.addFieldOffset(6, modedataOffset, 0);
    }

    static endNetLeaderboardEntry(builder: flatbuffers.Builder): flatbuffers.Offset {
        const offset = builder.endObject();
        return offset;
    }
}
