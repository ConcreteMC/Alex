using MiNET.Net;

namespace Alex.Net.Bedrock.Packets
{
	public class AdventureSettings : McpeAdventureSettings
	{
		/// <inheritdoc />
		protected override void DecodePacket()
		{
			this.Id = this.IsMcpe ? (byte) this.ReadVarInt() : this.ReadByte();
			this.flags = this.ReadUnsignedVarInt();
			this.commandPermission = this.ReadUnsignedVarInt();
			this.actionPermissions = this.ReadUnsignedVarInt();
			this.permissionLevel = this.ReadUnsignedVarInt();
			this.customStoredPermissions = this.ReadUnsignedVarInt();
			this.userId = this.ReadLong();
		}
	}
}