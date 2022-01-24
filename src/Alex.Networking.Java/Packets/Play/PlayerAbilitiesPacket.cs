using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class PlayerAbilitiesPacket : Packet<PlayerAbilitiesPacket>
	{
		public PlayerAbilitiesPacket()
		{
			PacketId = 0x19;
		}

		public byte Flags;
		public float FlyingSpeed;
		public float WalkingSpeed;
		public float FiedOfViewModifier;

		public bool ServerBound = false;

		public override void Decode(MinecraftStream stream)
		{
			Flags = (byte)stream.ReadByte();
			FlyingSpeed = stream.ReadFloat();
			FiedOfViewModifier = stream.ReadFloat();
		}

		public override void Encode(MinecraftStream stream)
		{
			if (ServerBound)
			{
				stream.WriteByte(Flags);
				//stream.WriteFloat(FlyingSpeed);
				//	stream.WriteFloat(WalkingSpeed);
			}
		}

		/// <inheritdoc />
		protected override void ResetPacket()
		{
			base.ResetPacket();
			ServerBound = false;
		}
	}
}