using System;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models
{
	public class ModelMeshPart : IDisposable
	{
		private Microsoft.Xna.Framework.Graphics.Effect _effect;

		/// <summary>
		///		The effect to use for drawing this mesh
		/// </summary>
		public Microsoft.Xna.Framework.Graphics.Effect Effect 
		{
			get 
			{
				return _effect;
			}
			set 
			{
				if (value == _effect)
					return;

				if (_effect != null)
				{
					// First check to see any other parts are also using this effect.
					var removeEffect = true;
					foreach (var part in Parent.MeshParts)
					{
						if (part != this && part._effect == _effect)
						{
							removeEffect = false;
							break;
						}
					}

					if (removeEffect)
						Parent.Effects.Remove(_effect);
				}

				// Set the new effect.
				_effect = value;
                
				if (_effect != null && !Parent.Effects.Contains(_effect))                
					Parent.Effects.Add(_effect);
			}
		}

		/// <summary>
		///		The indexbuffer containing the indices to the vertexbuffer elements
		/// </summary>
		public IndexBuffer IndexBuffer
		{
			get;
			set;
		}

		/// <summary>
		///		The number of vertices in this mesh
		/// </summary>
		public int NumVertices { get; set; }

		/// <summary>
		///		The amount of primitives to draw
		/// </summary>
		public int PrimitiveCount { get; set; }

		/// <summary>
		///		The offset used in the Index Buffer
		/// </summary>
		public int StartIndex { get; set; }

		/// <summary>
		///		Gets or sets an object identifying this mesh.
		/// </summary>
		public object Tag { get; set; }
		
		/// <summary>
		///		The vertex buffer containing the vertices for this mesh
		/// </summary>
		public VertexBuffer VertexBuffer
		{
			get;
			set;
		}

		/// <summary>
		///		Used to offset the vertex range indexed from the vertex buffer.
		/// </summary>
		public int VertexOffset { get; set; }

		internal ModelMesh Parent;

		/// <inheritdoc />
		public void Dispose()
		{
			if (VertexBuffer != null && !VertexBuffer.IsDisposed)
			{
				VertexBuffer?.Dispose();
				VertexBuffer = null;
			}

			if (IndexBuffer != null && !IndexBuffer.IsDisposed)
			{
				IndexBuffer?.Dispose();
				IndexBuffer = null;
			}

			_effect?.Dispose();
			_effect = null;
			Parent = null;
		}
	}
}