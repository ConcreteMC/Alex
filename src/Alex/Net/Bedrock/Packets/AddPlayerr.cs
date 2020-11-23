using System;
using MiNET.Net;
using NLog;

namespace Alex.Net.Bedrock
{
	public class AddPlayer : McpeAddPlayer
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(AddPlayer));
		
		/// <inheritdoc />
		protected override void DecodePacket()
		{
			this.Id = this.ReadByte();
				
			this.uuid = this.ReadUUID();
			this.username = this.ReadString();
			this.entityIdSelf = this.ReadSignedVarLong();
			this.runtimeEntityId = this.ReadUnsignedVarLong();
			this.platformChatId = this.ReadString();
			this.x = this.ReadFloat();
			this.y = this.ReadFloat();
			this.z = this.ReadFloat();
			this.speedX = this.ReadFloat();
			this.speedY = this.ReadFloat();
			this.speedZ = this.ReadFloat();
			this.pitch = this.ReadFloat();
			this.yaw = this.ReadFloat();
			this.headYaw = this.ReadFloat();
			
			try
			{
				this.item = this.AlternativeReadItem(false);
				this.metadata = this.ReadMetadataDictionaryAlternate();
				this.flags = this.ReadUnsignedVarInt();
			}
			catch (Exception error)
			{
				Log.Error(error, $"Could not fully decode AddPlayer packet: {error.ToString()}");
			}
		}
	}
}