using Alex.API.Blocks;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics
{
    public struct BlockShaderVertex : IVertexType
    {
	    public static readonly BlockShaderVertex Default = new BlockShaderVertex(Vector3.Zero, Vector3.Zero, Vector2.Zero, Color.White);
	    
        /// <summary>
        ///     Stores the position of this vertex
        /// </summary>
        public Vector3 Position;

        /// <summary>
        ///     The UV co-ords for this vertex (the co-ords in the texture)
        /// </summary>
        public Vector2 TexCoords;

        /// <summary>
        ///     The color of this vertex
        /// </summary>
        public Color Color;

        public float BlockLight;

        public float SkyLight;

        public BlockFace Face;
        
        /// <summary>
        ///     Creates a new VertexPositionNormalTextureColor
        /// </summary>
        /// <param name="position">The position in space for this vertex</param>
        /// <param name="normal">The nomal for this vector</param>
        /// <param name="texCoords">The UV co-ords for this vertex</param>
        public BlockShaderVertex(Vector3 position, Vector3 normal, Vector2 texCoords)
        {
            Position = position;
            TexCoords = texCoords;
            Color = Color.White;
            BlockLight = 0f;
            SkyLight = 15f;
			Face = BlockFace.None;
        }

        //public Vector3 Normal;

        /// <summary>
        ///     Creates a new VertexPositionNormalTextureColor
        /// </summary>
        /// <param name="position">The position in space for this vertex</param>
        /// <param name="normal">The nomal for this vector</param>
        /// <param name="texCoords">The UV co-ords for this vertex</param>
        /// <param name="color">The color of this vertex</param>
        public BlockShaderVertex(Vector3 position, Vector3 normal, Vector2 texCoords, Color color)
        {
            Position = position;
            //Normal = normal;
            TexCoords = texCoords;
            Color = color;
            BlockLight = 0f;
            SkyLight = 15f;
            Face = BlockFace.None;
        }

        /// <summary>
        ///     The vertex declaration for this vertex type
        /// </summary>
        public static VertexDeclaration VertexDeclaration { get; } = new VertexDeclaration
        (
	        new VertexElement(0, VertexElementFormat.Vector3,
		        VertexElementUsage.Position, 0),
	        new VertexElement(3 * sizeof(float), VertexElementFormat.Vector2,
		        VertexElementUsage.TextureCoordinate, 0),
	        new VertexElement(5 * sizeof(float), VertexElementFormat.Color,
		        VertexElementUsage.Color, 0),
	        new VertexElement((5 * sizeof(float)) + 4 * sizeof(byte), VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
	        new VertexElement((6 * sizeof(float)) + 4 * sizeof(byte), VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
	        new VertexElement((7 * sizeof(float)) + 4 * sizeof(byte), VertexElementFormat.Single, VertexElementUsage.Position,1)
        );

        public static int             BlockLightOffset = (5 * sizeof(float)) + 4 * sizeof(byte);
        public static int             SkyLightOffset = (6 * sizeof(float)) + 4 * sizeof(byte);
        
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}
