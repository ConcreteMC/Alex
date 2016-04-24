using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Rendering
{
    public class ObjectManager : Singleton<ObjectManager>
    {
        public ObjectManager()
        {
            Chunks = new Dictionary<Vector3, Chunk>();

			Effect = new AlphaTestEffect(Game.Instance.GraphicsDevice)
            {
                Texture = ResManager.GetAtlas(),
                World = Matrix.Identity,
                VertexColorEnabled = true,
            };
        }

        public Dictionary<Vector3, Chunk> Chunks { get; }
		 
        private AlphaTestEffect Effect { get; }

        public int Vertices { get; private set; }
        public void Draw(GraphicsDevice device)
        {
            Effect.View = Game.MainCamera.ViewMatrix;
            Effect.Projection = Game.MainCamera.ProjectionMatrix;

            var tempVertices = 0;
            foreach (var c in Chunks.ToArray())
            {
                var chunk = c.Value;

               // var boundingSphere = new BoundingBox(chunk.Position, chunk.Position + Chunk.ChunkSize);
               // if (Game.MainCamera.BoundingFrustum.Contains(boundingSphere) == ContainmentType.Disjoint) continue;
                
                if (chunk.VertexBuffer == null)
                {
                    chunk.Mesh = chunk.GenerateMesh();
                    chunk.VertexBuffer = new VertexBuffer(device, VertexPositionNormalTextureColor.VertexDeclaration,
                        chunk.Mesh.Vertices.Length,
                        BufferUsage.WriteOnly);
                }

	            if (chunk.IsDirty)
	            {
		            chunk.Mesh = chunk.GenerateMesh();
		            chunk.VertexBuffer = new VertexBuffer(device, VertexPositionNormalTextureColor.VertexDeclaration,
			            chunk.Mesh.Vertices.Length,
			            BufferUsage.WriteOnly);

		            if (chunk.Mesh.Vertices.Length > 0)
		            {
			            chunk.VertexBuffer.SetData(chunk.Mesh.Vertices);
		            }

		            chunk.IsDirty = false;
	            }

	            if (chunk.Mesh.Vertices.Length == 0) continue;

                device.SetVertexBuffer(chunk.VertexBuffer);
                foreach (var pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, chunk.Mesh.Vertices.Length / 3);
                }
                tempVertices += chunk.Mesh.Vertices.Length;
            }
            Vertices = tempVertices;
        }

        public void AddChunk(Chunk chunk, Vector3 position)
        {
            if (Chunks.ContainsKey(position)) return;

            Chunks.Add(position, chunk);
        }

        public void RemoveChunk(Vector3 position)
        {
            if (!Chunks.ContainsKey(position)) return;

            Chunks.Remove(position);
        }
    }
}