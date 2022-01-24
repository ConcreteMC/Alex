using System;
using System.Threading.Tasks;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class LightingData
	{
		public bool TrustEdges;

		public BitSet SkyLightMask;

		public BitSet BlockLightMask;

		public BitSet EmptySkyLightMask;

		public BitSet EmptyBlockLightMask;

		public byte[][] SkyLight;
		public byte[][] BlockLight;

		public LightingData() { }

		public async Task DecodeAsync(MinecraftStream stream)
		{
			TrustEdges = await stream.ReadBoolAsync();
			SkyLightMask = await BitSet.ReadAsync(stream);
			BlockLightMask = await BitSet.ReadAsync(stream);
			EmptySkyLightMask = await BitSet.ReadAsync(stream);
			EmptyBlockLightMask = await BitSet.ReadAsync(stream);

			int skyLightArrayCount = await stream.ReadVarIntAsync();
			SkyLight = new byte[skyLightArrayCount][];

			for (int idx = 0; idx < SkyLight.Length; idx++)
			{
				int length = await stream.ReadVarIntAsync();
				SkyLight[idx] = await stream.ReadAsync(length);
			}

			int blockLightArrayCount = await stream.ReadVarIntAsync();
			BlockLight = new byte[blockLightArrayCount][];

			for (int idx = 0; idx < BlockLight.Length; idx++)
			{
				int length = await stream.ReadVarIntAsync();
				BlockLight[idx] = await stream.ReadAsync(length);
			}
		}

		public static async Task<LightingData> FromStreamAsync(MinecraftStream stream)
		{
			var data = new LightingData();
			await data.DecodeAsync(stream);

			return data;
		}
	}

	public class UpdateLightPacket : Packet<UpdateLightPacket>
	{
		public int ChunkX, ChunkZ;
		public LightingData Data;

		public UpdateLightPacket() { }

		/// <inheritdoc />
		public override async Task DecodeAsync(MinecraftStream stream)
		{
			ChunkX = await stream.ReadVarIntAsync();
			ChunkZ = await stream.ReadVarIntAsync();
			Data = await LightingData.FromStreamAsync(stream);
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		protected override void ResetPacket()
		{
			base.ResetPacket();
			Data = null;
		}
	}
}