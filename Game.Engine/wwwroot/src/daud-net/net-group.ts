// automatically generated by the FlatBuffers compiler, do not modify

import * as flatbuffers from 'flatbuffers';

export class NetGroup {
  bb: flatbuffers.ByteBuffer|null = null;
  bb_pos = 0;
__init(i:number, bb:flatbuffers.ByteBuffer):NetGroup {
  this.bb_pos = i;
  this.bb = bb;
  return this;
}

static getRootAsNetGroup(bb:flatbuffers.ByteBuffer, obj?:NetGroup):NetGroup {
  return (obj || new NetGroup()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
}

static getSizePrefixedRootAsNetGroup(bb:flatbuffers.ByteBuffer, obj?:NetGroup):NetGroup {
  bb.setPosition(bb.position() + flatbuffers.SIZE_PREFIX_LENGTH);
  return (obj || new NetGroup()).__init(bb.readInt32(bb.position()) + bb.position(), bb);
}

group():number {
  const offset = this.bb!.__offset(this.bb_pos, 4);
  return offset ? this.bb!.readUint32(this.bb_pos + offset) : 0;
}

type():number {
  const offset = this.bb!.__offset(this.bb_pos, 6);
  return offset ? this.bb!.readUint8(this.bb_pos + offset) : 0;
}

caption():string|null
caption(optionalEncoding:flatbuffers.Encoding):string|Uint8Array|null
caption(optionalEncoding?:any):string|Uint8Array|null {
  const offset = this.bb!.__offset(this.bb_pos, 8);
  return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
}

zindex():number {
  const offset = this.bb!.__offset(this.bb_pos, 10);
  return offset ? this.bb!.readUint32(this.bb_pos + offset) : 0;
}

owner():number {
  const offset = this.bb!.__offset(this.bb_pos, 12);
  return offset ? this.bb!.readUint32(this.bb_pos + offset) : 0;
}

color():string|null
color(optionalEncoding:flatbuffers.Encoding):string|Uint8Array|null
color(optionalEncoding?:any):string|Uint8Array|null {
  const offset = this.bb!.__offset(this.bb_pos, 14);
  return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
}

customdata():string|null
customdata(optionalEncoding:flatbuffers.Encoding):string|Uint8Array|null
customdata(optionalEncoding?:any):string|Uint8Array|null {
  const offset = this.bb!.__offset(this.bb_pos, 16);
  return offset ? this.bb!.__string(this.bb_pos + offset, optionalEncoding) : null;
}

static startNetGroup(builder:flatbuffers.Builder) {
  builder.startObject(7);
}

static addGroup(builder:flatbuffers.Builder, group:number) {
  builder.addFieldInt32(0, group, 0);
}

static addType(builder:flatbuffers.Builder, type:number) {
  builder.addFieldInt8(1, type, 0);
}

static addCaption(builder:flatbuffers.Builder, captionOffset:flatbuffers.Offset) {
  builder.addFieldOffset(2, captionOffset, 0);
}

static addZindex(builder:flatbuffers.Builder, zindex:number) {
  builder.addFieldInt32(3, zindex, 0);
}

static addOwner(builder:flatbuffers.Builder, owner:number) {
  builder.addFieldInt32(4, owner, 0);
}

static addColor(builder:flatbuffers.Builder, colorOffset:flatbuffers.Offset) {
  builder.addFieldOffset(5, colorOffset, 0);
}

static addCustomdata(builder:flatbuffers.Builder, customdataOffset:flatbuffers.Offset) {
  builder.addFieldOffset(6, customdataOffset, 0);
}

static endNetGroup(builder:flatbuffers.Builder):flatbuffers.Offset {
  const offset = builder.endObject();
  return offset;
}

static createNetGroup(builder:flatbuffers.Builder, group:number, type:number, captionOffset:flatbuffers.Offset, zindex:number, owner:number, colorOffset:flatbuffers.Offset, customdataOffset:flatbuffers.Offset):flatbuffers.Offset {
  NetGroup.startNetGroup(builder);
  NetGroup.addGroup(builder, group);
  NetGroup.addType(builder, type);
  NetGroup.addCaption(builder, captionOffset);
  NetGroup.addZindex(builder, zindex);
  NetGroup.addOwner(builder, owner);
  NetGroup.addColor(builder, colorOffset);
  NetGroup.addCustomdata(builder, customdataOffset);
  return NetGroup.endNetGroup(builder);
}
}
