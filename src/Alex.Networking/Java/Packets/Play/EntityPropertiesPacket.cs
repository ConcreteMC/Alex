using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Utils;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityPropertiesPacket : Packet<EntityPropertiesPacket>
	{
		public int                                EntityId { get; set; }
		public Dictionary<string, EntityProperty> Properties = new Dictionary<string, EntityProperty>();

		public EntityPropertiesPacket() { }

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			int count = stream.ReadInt();

			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadString();
				double value = stream.ReadDouble();

				int        propCount = stream.ReadVarInt();
				Modifier[] modifiers = new Modifier[propCount];

				for (int y = 0; y < modifiers.Length; y++)
				{
					UUID         uuid   = new UUID(stream.ReadUuid().ToByteArray());
					double       amount = stream.ReadDouble();
					ModifierMode op     = (ModifierMode) stream.ReadByte();

					modifiers[y] = EntityProperty.Factory.CreateModifier(uuid, amount, op);
				}

				EntityProperty prop = EntityProperty.Factory.Create(key, value, modifiers);

				if (!Properties.ContainsKey(prop.Key))
				{
					Properties.Add(prop.Key, prop);
				}
			}
		}

		public override void Encode(MinecraftStream stream) { }
	}

	public class EntityPropertyFactory
	{
		public virtual EntityProperty Create(string key, double value, Modifier[] modifiers)
		{
			return new EntityProperty(key, value, modifiers);
		}

		public virtual Modifier CreateModifier(UUID uuid, double amount, ModifierMode modifierMode)
		{
			return new Modifier(uuid, amount, modifierMode);
		}
	}

	public static class EntityProperties
	{
		public const string FlyingSpeed   = "generic.flying_speed";
		public const string MovementSpeed = "generic.movement_speed";
	}

	public class EntityProperty
	{
		public static EntityPropertyFactory Factory { get; set; } = new EntityPropertyFactory();

		public string     Key { get; }
		public double     Value { get; set; }
		public List<Modifier> Modifiers { get; }

		public EntityProperty(string key, double value, Modifier[] modifiers)
		{
			Key = key;
			Value = value;
			Modifiers = new List<Modifier>();

			if (modifiers != null)
				Modifiers.AddRange(modifiers);
		}
		
		protected virtual IEnumerable<Modifier> GetAppliedModifiers()
		{
			return Modifiers;
		}
		
		public virtual double Calculate()
		{
			var modifiers = GetAppliedModifiers().ToArray();
			
			var baseValue = Value;
			foreach (var modifier in modifiers.Where(modifier => modifier.Operation == ModifierMode.Add))
			{
				baseValue += modifier.Amount;
			}

			var value = baseValue;

			foreach (var modifier in modifiers.Where(modifier => modifier.Operation == ModifierMode.AddMultiplied))
			{
				value += baseValue * modifier.Amount;
			}

			foreach (var modifier in modifiers.Where(modifier => modifier.Operation == ModifierMode.Multiply))
			{
				value *= 1d + modifier.Amount;
			}

			return value;
		}
	}

	public class Modifier
	{
		public UUID         Uuid;
		public double       Amount;
		public ModifierMode Operation;

		public Modifier() { }

		public Modifier(UUID uuid, double amount, ModifierMode mode)
		{
			Uuid = uuid;
			Amount = amount;
			Operation = mode;
		}
	}

	public enum ModifierMode
	{
		Add          = 0,
		AddMultiplied = 1,
		Multiply     = 2
	}
}
