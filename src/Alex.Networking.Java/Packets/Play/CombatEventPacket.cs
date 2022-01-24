using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class CombatEventPacket : Packet<CombatEventPacket>
	{
		public CombatEvent Event { get; set; }
		public int Duration { get; set; }
		public int EntityId;
		public int PlayerId;
		public string Message;

		public override void Decode(MinecraftStream stream)
		{
			Event = (CombatEvent)stream.ReadVarInt();

			switch (Event)
			{
				case CombatEvent.EnterCombat:

					break;

				case CombatEvent.EndCombat:
					Duration = stream.ReadVarInt();
					EntityId = stream.ReadInt();

					break;

				case CombatEvent.EntityDead:
					PlayerId = stream.ReadVarInt();
					EntityId = stream.ReadInt();
					Message = stream.ReadString();

					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}

		public enum CombatEvent
		{
			EnterCombat = 0,
			EndCombat = 1,
			EntityDead = 2
		}
	}
}