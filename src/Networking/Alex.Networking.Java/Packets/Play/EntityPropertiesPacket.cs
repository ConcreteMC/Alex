using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityPropertiesPacket : Packet<EntityPropertiesPacket>
	{
		public int EntityId { get; set; }
		public Dictionary<string, EntityProperty> Properties = new Dictionary<string, EntityProperty>();

		public EntityPropertiesPacket() { }

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			int count = stream.ReadVarInt();

			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadString();
				double value = stream.ReadDouble();

				int propCount = stream.ReadVarInt();
				Modifier[] modifiers = new Modifier[propCount];

				for (int y = 0; y < modifiers.Length; y++)
				{
					var uuid = stream.ReadUuid();
					double amount = stream.ReadDouble();
					ModifierMode op = (ModifierMode)stream.ReadByte();

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

		public virtual Modifier CreateModifier(Guid uuid, double amount, ModifierMode modifierMode)
		{
			return new Modifier(uuid, amount, modifierMode);
		}
	}

	public static class EntityProperties
	{
		public const string FlyingSpeed = "generic.flying_speed";
		public const string MovementSpeed = "generic.movement_speed";
		public const string GenericMovementSpeed = "minecraft:generic.movement_speed";
		public const string AttackSpeed = "generic.attack_speed	";
	}

	public class EntityProperty
	{
		public static EntityPropertyFactory Factory { get; set; } = new EntityPropertyFactory();

		public string Key { get; }
		public double Value { get; set; }
		private ConcurrentDictionary<Guid, Modifier> Modifiers { get; }

		public EntityProperty(string key, double value, Modifier[] modifiers)
		{
			Key = key;
			Value = value;
			Modifiers = new ConcurrentDictionary<Guid, Modifier>();

			if (modifiers != null)
			{
				foreach (var modifier in modifiers)
				{
					Modifiers.TryAdd(modifier.Uuid, modifier);
				}
			}
			//Modifiers.AddRange(modifiers);
		}

		public void ApplyModifier(Modifier modifier)
		{
			if (!Modifiers.TryAdd(modifier.Uuid, modifier))
			{
				if (Modifiers.TryGetValue(modifier.Uuid, out var currentModifier))
				{
					currentModifier.Amount = modifier.Amount;
					currentModifier.Operation = modifier.Operation;
					//	currentModifier.Uuid = modifier.Uuid;
				}
			}
		}

		public void RemoveModifier(Guid key)
		{
			if (Modifiers.TryRemove(key, out _)) { }
		}

		/*protected virtual IEnumerable<Modifier> GetAppliedModifiers()
		{
			foreach (var modifier in Modifiers)
			{
				yield return modifier.Value;
			}
			//return Modifiers.Values.ToArray();
		}*/

		public virtual double Calculate()
		{
			if (Modifiers.Count == 0)
				return Value;

			var modifiers = Modifiers.Values.ToArray();
			//var modifiers = GetAppliedModifiers().ToArray();

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
		public Guid Uuid;
		public double Amount;
		public ModifierMode Operation;

		public Modifier() { }

		public Modifier(Guid uuid, double amount, ModifierMode mode)
		{
			Uuid = uuid;
			Amount = amount;
			Operation = mode;
		}
	}
	
	public enum ModifierMode
	{
		Add = 0,
		AddMultiplied = 1,
		Multiply = 2
	}
}