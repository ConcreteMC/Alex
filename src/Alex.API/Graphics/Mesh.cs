using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alex.API.Graphics
{
	public class Mesh
	{
		public Mesh()
		{
		}

		public Mesh(VertexPositionNormalTextureColor[] vertices)
		{
			Vertices = vertices;
		}

		public VertexPositionNormalTextureColor[] Vertices { get; set; }
	}
}
