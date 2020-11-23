using MiNET.Net;

namespace Alex.Net.Bedrock
{
	public class ClientCacheBlobStatus : Packet<ClientCacheBlobStatus>
	{
		public ClientCacheBlobStatus()
		{
			this.Id = (byte) 135;
			this.IsMcpe = true;
		}

		/// <inheritdoc />
		protected override void EncodePacket()
		{
			base.EncodePacket();
		/*	
			WriteUnsignedVarInt(missingBlobs.length);

			for (PocketFullChunkData.Blob missingBlob : missingBlobs )
			{
				buf.writeLong( missingBlob.getBlobId() );
				buf.writeVarArray( missingBlob.getBlobData() );
			}*/
		}
	}
}