using System;
using Alex.Common.Data;
using Alex.Networking.Java.Util;
using Microsoft.Xna.Framework;

namespace Alex.Networking.Java.Packets.Play
{
	public class ParticlePacket : Packet<ParticlePacket>
	{
		public static Func<int, string> RegistryLookup = null;
		
		public int ParticleId;
		public bool LongDistance;
		public double X;
		public double Y;
		public double Z;
		public float OffsetX;
		public float OffsetY;
		public float OffsetZ;
		public float ParticleData;
		public int ParticleCount;

		public Color? Color = null;
		public float Scale = 1f;

		public SlotData SlotData = null;

		public int? BlockStateId = null;
		
		public override void Decode(MinecraftStream stream)
		{
			ParticleId = stream.ReadInt();
			LongDistance = stream.ReadBool();
			X = stream.ReadDouble();
			Y = stream.ReadDouble();
			Z = stream.ReadDouble();
			OffsetX = stream.ReadFloat();
			OffsetY = stream.ReadFloat();
			OffsetZ = stream.ReadFloat();
			ParticleData = stream.ReadFloat();
			ParticleCount = stream.ReadInt();

			var key = RegistryLookup?.Invoke(ParticleId);

			if (string.IsNullOrWhiteSpace(key))
				return;

			switch (key)
			{
				case "minecraft:dust":
					var r = stream.ReadFloat();
					var g = stream.ReadFloat();
					var b = stream.ReadFloat();
					Color = new Color(r, g, b);
					Scale = stream.ReadFloat();
					break;
				case "minecraft:item":
					SlotData = stream.ReadSlot();
					break;
				case "minecraft:block":
					BlockStateId = stream.ReadVarInt();
					break;
			}
			//TODO: Read data, varies per particle tho...
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		protected override void ResetPacket()
		{
			base.ResetPacket();
			
			Color = null;
			Scale = 1f;
			SlotData = null;
			BlockStateId = null;
		}
	}
}
