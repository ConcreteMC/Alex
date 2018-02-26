namespace Alex.CoreRT.API.Graphics
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
