using System.Collections.Generic;
using Alex.API.Utils;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class EntityPropertiesPacket : Packet<EntityPropertiesPacket>
    {
		public int EntityId { get; set; }
	    public Dictionary<string, Property> Properties = new Dictionary<string, Property>();

	    public EntityPropertiesPacket()
	    {

	    }

	    public override void Decode(MinecraftStream stream)
	    {
		    EntityId = stream.ReadVarInt();
		    int count = stream.ReadInt();
		    for (int i = 0; i < count; i++)
		    {
				Property prop = new Property();
			    string key = stream.ReadString();
			    double value = stream.ReadDouble();

			    int propCount = stream.ReadVarInt();
				Modifier[] modifiers = new Modifier[propCount];
			    for (int y = 0; y < modifiers.Length; y++)
			    {
					UUID uuid = new UUID(stream.ReadUuid().ToByteArray());
				    double amount = stream.ReadDouble();
				    byte op = (byte)stream.ReadByte();

					modifiers[y] = new Modifier()
					{
						Amount = amount,
						Operation = op,
						Uuid = uuid
					};
			    }

			    prop.Value = value;
			    prop.Modifiers = modifiers;
			    prop.Key = key;

			    if (!Properties.ContainsKey(key))
			    {
				    Properties.Add(key, prop);
			    }
		    }
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    
	    }

	    public class Property
	    {
		    public string Key;
		    public double Value;
		    public Modifier[] Modifiers;
	    }

	    public class Modifier
	    {
		    public UUID Uuid;
		    public double Amount;
		    public byte Operation;
	    }
    }
}
