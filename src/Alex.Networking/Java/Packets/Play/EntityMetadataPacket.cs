using System;
using System.Collections.Generic;
using System.IO;
using Alex.API.Data;
using Alex.API.Utils;
using Alex.Networking.Java.Util;
using Microsoft.Xna.Framework;

namespace Alex.Networking.Java.Packets.Play
{
    public class EntityMetadataPacket : Packet<EntityMetadataPacket>
    {
	    public int EntityId;
	    public List<MetaDataEntry> Entries = new List<MetaDataEntry>();
		//public 
	    public EntityMetadataPacket()
	    {

	    }
	
	    private byte[] Data { get; set; }
	    public override void Decode(MinecraftStream stream)
	    {
		    EntityId = stream.ReadVarInt();
		    Data = stream.Read((int) (stream.Length - stream.Position));
		    //TODO: Read metadata properly
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    throw new NotImplementedException();
	    }

	    public void FinishReading()
	    {
		    using(MemoryStream ms = new MemoryStream(Data))
		    using (MinecraftStream stream = new MinecraftStream(ms))
		    {
			    byte index = 0;
			    do
			    {
				    index = (byte) stream.ReadByte();
				    if (index == 0xff)
				    {
					    break;
				    }

				    MetadataType type = (MetadataType) stream.ReadVarInt();
				    MetaDataEntry meta = null;
				    switch (type)
				    {
					    case MetadataType.Byte:
						    meta = new MetadataByte(index, (byte) stream.ReadByte());
						    break;
					    case MetadataType.Varint:
						    meta = new MetadataVarInt(index,  stream.ReadVarInt());
						    break;
					    case MetadataType.Float:
						    meta = new MetadataFloat(index, stream.ReadFloat());
						    break;
					    case MetadataType.String:
						    meta = new MetadataString(index, stream.ReadString());
						    break;
					    case MetadataType.Chat:
						    meta = new MetadataChat(index, stream.ReadChatObject());
						    break;
					    case MetadataType.OptChat:
						    var hasData = stream.ReadBool();
						    meta = new MetadataOptChat(index, hasData, hasData ? stream.ReadChatObject() : null);
						    break;
					    case MetadataType.Slot:
						    meta = new MetadataSlot(index, stream.ReadSlot());
						    break;
					    case MetadataType.Boolean:
						    meta =new MetadataBool(index, stream.ReadBool());
						    break;
					    case MetadataType.Rotation:
						    meta = new MetadataRotation(index, new Vector3(stream.ReadFloat(),stream.ReadFloat(), stream.ReadFloat()));
						    break;
					    case MetadataType.Position:
						    stream.ReadPosition();
						    break;
					    case MetadataType.OptPosition:
						    if (stream.ReadBool())
							    stream.ReadPosition();
						    break;
					    case MetadataType.Direction:
						    stream.ReadVarInt();
						    break;
					    case MetadataType.OptUUID:
						    if (stream.ReadBool())
							    stream.ReadUuid();
						    break;
					    case MetadataType.OptBlockID:
						    stream.ReadVarInt();
						    break;
					    case MetadataType.NBT:
						    stream.ReadNbtCompound();
						    break;
					    case MetadataType.Particle:
						    break;
					    case MetadataType.VillagerData:
						    stream.ReadVarInt();
						    stream.ReadVarInt();
						    stream.ReadVarInt();
						    break;
					    case MetadataType.OptVarInt:
						    stream.ReadVarInt();
						    break;
					    case MetadataType.Pose:
						    stream.ReadVarInt();
						    break;
				    }

				    if (meta != null)
				    {
					    Entries.Add(meta);
				    }
				    
			    } while (index != 0xff);
		    }
	    }
    }

    public enum MetadataType
    {
	    Byte = 0,
	    Varint,
	    Float,
	    String,
	    Chat,
	    OptChat,
	    Slot,
	    Boolean,
	    Rotation,
	    Position,
	    OptPosition,
	    Direction,
	    OptUUID,
	    OptBlockID,
	    NBT,
	    Particle,
	    VillagerData,
	    OptVarInt,
	    Pose
    }
    
    public class MetaDataEntry
    {
	    public byte Index { get; set; }
	    public MetadataType Type { get; set; }
		
	    public MetaDataEntry(byte index, MetadataType type)
	    {
		    Index = index;
		    Type = type;
	    }
    }

    public class MetadataByte : MetaDataEntry
    {
	    public byte Value { get; set; }
	    public MetadataByte(byte index, byte value) : base(index, MetadataType.Byte)
	    {
		    Value = value;
	    }
    }
    
    public class MetadataBool : MetaDataEntry
    {
	    public bool Value { get; set; }
	    public MetadataBool(byte index, bool value) : base(index, MetadataType.Boolean)
	    {
		    Value = value;
	    }
    }
    
    public class MetadataVarInt : MetaDataEntry
    {
	    public int Value { get; set; }
	    public MetadataVarInt(byte index, int value) : base(index, MetadataType.Varint)
	    {
		    Value = value;
	    }
    }
    
    public class MetadataFloat : MetaDataEntry
    {
	    public float Value { get; set; }
	    public MetadataFloat(byte index, float value) : base(index, MetadataType.Float)
	    {
		    Value = value;
	    }
    }
    
    public class MetadataString : MetaDataEntry
    {
	    public string Value { get; set; }
	    public MetadataString(byte index, string value) : base(index, MetadataType.String)
	    {
		    Value = value;
	    }
    }

    public class MetadataRotation : MetaDataEntry
    {
	    public Vector3 Rotation { get; }
	    
	    /// <inheritdoc />
	    public MetadataRotation(byte index, Vector3 rotation) : base(index, MetadataType.Rotation)
	    {
		    Rotation = rotation;
	    }
    }
    
    public class MetadataSlot : MetaDataEntry
    {
	    public SlotData Value { get; set; }
	    public MetadataSlot(byte index, SlotData value) : base(index, MetadataType.Slot)
	    {
		    Value = value;
	    }
    }
    
    public class MetadataBlockId : MetaDataEntry
    {
	    public uint Value { get; set; }
	    public MetadataBlockId(byte index, uint value) : base(index, MetadataType.OptBlockID)
	    {
		    Value = value;
	    }
    }

    public class MetadataOptChat : MetaDataEntry
    {
	    public bool HasValue { get; set; }
	    public ChatObject Value { get; set; }
	    
	    public MetadataOptChat(byte index, bool hasValue, ChatObject value) : base(index, MetadataType.OptChat)
	    {
		    HasValue = hasValue;
		    Value = value;
	    }
    }
    
    public class MetadataChat : MetaDataEntry
    {
	    public ChatObject Value { get; set; }
	    
	    public MetadataChat(byte index, ChatObject value) : base(index, MetadataType.OptChat)
	    {
		    Value = value;
	    }
    }
}
