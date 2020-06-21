using MiNET.Net;

namespace Alex.Net.Bedrock
{
	public class SetEntityData : McpeSetEntityData
	{
		/// <inheritdoc />
		protected override void DecodePacket()
		{
			this.Id = ReadByte();
			this.runtimeEntityId = this.ReadUnsignedVarLong();
			this.metadata = this.ReadMetadataDictionaryAlternate();
		}
	}
}