using System;
using System.Collections.Generic;
using System.IO;
using Alex.API.Blocks;
using Alex.API.Data;
using Alex.API.Utils;
using Alex.Networking.Java.Util;
using fNbt;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using MiNET.Entities;

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
						    meta = new MetadataPosition(index, stream.ReadPosition());
						    break;

					    case MetadataType.OptPosition:
					    {
						    bool hasPosition = stream.ReadBool();

						    meta = new MetadataOptPosition(
							    index, hasPosition, hasPosition ? stream.ReadPosition() : (Vector3?) null);
					    }

						    break;

					    case MetadataType.Direction:
						    meta = new MetadataDirection(index, (API.Utils.Direction) stream.ReadVarInt());// stream.ReadVarInt();
						    break;

					    case MetadataType.OptUUID:
					    {
						    var hasUUID = stream.ReadBool();
						    meta = new MetadataOptUUID(index, hasUUID, hasUUID ? new UUID(stream.ReadUuid().ToByteArray()) : null);// stream.ReadUuid();
					    }
						    break;

					    case MetadataType.OptBlockID:
						    stream.ReadVarInt();
						    break;
					    case MetadataType.NBT:
						    meta = new MetadataNbt(index, stream.ReadNbtCompound());
						    break;
					    case MetadataType.Particle:
						    break;
					    case MetadataType.VillagerData:
						    meta = new MetadataVillagerData(
							    index, (MetadataVillagerData.VillagerTypes) stream.ReadVarInt(), (MetadataVillagerData.VillagerProfession) stream.ReadVarInt(), (MetadataVillagerData.VillagerLevel) stream.ReadVarInt());
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

    public class MetadataPosition : MetaDataEntry
    {
	    public Vector3 Position { get; set; }
	    
	    /// <inheritdoc />
	    public MetadataPosition(byte index, Vector3 position) : base(index, MetadataType.Position)
	    {
		    Position = position;
	    }
    }
    
    
    public class MetadataOptPosition : MetaDataEntry
    {
	    public bool HasValue { get; set; }
	    public Vector3? Position { get; set; }
	    
	    /// <inheritdoc />
	    public MetadataOptPosition(byte index, bool hasPosition, Vector3? position) : base(index, MetadataType.OptPosition)
	    {
		    HasValue = hasPosition;
		    Position = position;
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

    public class MetadataOptUUID : MetaDataEntry
    {
	    public bool HasValue { get; set; }
	    public UUID Value { get; set; }
	    
	    /// <inheritdoc />
	    public MetadataOptUUID(byte index, bool hasValue, UUID uuid) : base(index, MetadataType.OptUUID)
	    {
		    HasValue = hasValue;
		    Value = uuid;
	    }
    }

    public class MetadataNbt : MetaDataEntry
    {
	    public NbtCompound Compound { get; set; }
	    /// <inheritdoc />
	    public MetadataNbt(byte index, NbtCompound compound) : base(index, MetadataType.NBT)
	    {
		    Compound = compound;
	    }
    }

    public class MetadataDirection : MetaDataEntry
    {
	    public Alex.API.Utils.Direction Direction { get; set; }
	    /// <inheritdoc />
	    public MetadataDirection(byte index, Alex.API.Utils.Direction direction) : base(index, MetadataType.Direction)
	    {
		    Direction = direction;
	    }
    }

    public class MetadataVillagerData : MetaDataEntry
    {
	    public VillagerTypes VillagerType { get; set; }
	    public VillagerProfession Profession { get; set; }
	    public VillagerLevel Level { get; set; }
	    
	    /// <inheritdoc />
	    public MetadataVillagerData(byte index, VillagerTypes type, VillagerProfession profession, VillagerLevel level) : base(
		    index, MetadataType.VillagerData)
	    {
		    VillagerType = type;
		    Profession = profession;
		    Level = level;
	    }

	    public enum VillagerTypes
	    {
		    Desert = 0,
		    Jungle,
		    Plains,
		    Savanna,
		    Snow,
		    Swamp,
		    Taiga
	    }

	    public enum VillagerProfession
	    {
		    None = 0,
		    Armorer = 1,
		    Butcher = 2,
		    Cartographer = 3,
		    Cleric = 4,
		    Farmer = 5,
		    Fisherman = 6,
		    Fletcher = 7,
		    LeatherWorker = 8,
		    Librarian = 9,
		    Mason = 10,
		    Nitwit = 11,
		    Shepherd = 12,
		    Toolsmith  =13,
		    WeaponSmith = 14,
	    }

	    public enum VillagerLevel
	    {
		    Stone = 0,
		    Iron = 1,
		    Gold = 2,
		    Emerald = 3,
		    Diamond = 4
	    }
    }
}
