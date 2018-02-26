using System;
using Alex.CoreRT.API.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.CoreRT.API.World
{
	public interface IChunkColumn : IDisposable
	{
		IBlock GetBlock(int bx, int by, int bz);
		void SetBlock(int bx, int by, int bz, IBlock block);
		void SetHeight(int bx, int bz, short h);
		byte GetHeight(int bx, int bz);
		void SetBiome(int bx, int bz, byte biome);
		byte GetBiome(int bx, int bz);
		byte GetBlocklight(int bx, int by, int bz);
		void SetBlocklight(int bx, int by, int bz, byte data);
		byte GetSkylight(int bx, int by, int bz);
		void SetSkyLight(int bx, int by, int bz, byte data);
		void GenerateMeshes(IWorld world, out Mesh mesh, out Mesh transparentMesh);

		VertexBuffer VertexBuffer { get; set; }
		VertexBuffer TransparentVertexBuffer { get; set; }

		object VertexLock { get; set; }
		object UpdateLock { get; set; }
		bool IsDirty { get; set; }
		bool Scheduled { get; set; }
		int GetHeighest();
	}
}