using Alex.Common.Blocks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Alex.Common.Graphics
{
	public struct MinifiedBlockShaderVertex : IVertexType
    {
	    public static readonly MinifiedBlockShaderVertex Default = new MinifiedBlockShaderVertex(Vector3.Zero, BlockFace.None, Vector4.Zero, Color.White);
	    
        /// <summary>
        ///     Stores the position of this vertex
        /// </summary>
        public Vector3 Position;

		/// <summary>
		/// 	Stores the normal of this vertex
		/// </summary>
        public float Normal;

        /// <summary>
        ///     The UV co-ords for this vertex (the co-ords in the texture)
        /// </summary>
        public Vector4 TexCoords;

        /// <summary>
        ///     The color of this vertex
        /// </summary>
        public Color Color;

        public Short2 Lighting;
        
        /// <summary>
        ///     Creates a new MinifiedBlockShaderVertex
        /// </summary>
        /// <param name="position">The position in space for this vertex</param>
        /// <param name="normal">The normal for this vector</param>
        /// <param name="texCoords">The UV co-ords for this vertex</param>
        public MinifiedBlockShaderVertex(Vector3 position, BlockFace normal, Vector4 texCoords) : this(position, normal, texCoords, Color.White, 0, 15)
        {
	        //BlockLight = 0;
            //SkyLight = 15;
        }

        /// <summary>
        ///     Creates a new MinifiedBlockShaderVertex
        /// </summary>
        /// <param name="position">The position in space for this vertex</param>
        /// <param name="normal">The normal for this vector</param>
        /// <param name="texCoords">The UV co-ords for this vertex</param>
        /// <param name="color">The color of this vertex</param>
        public MinifiedBlockShaderVertex(Vector3 position, BlockFace normal, Vector4 texCoords, Color color) : this(position, normal, texCoords, color, 0, 15)
        {
           
        }

        /// <summary>
        ///     Creates a new MinifiedBlockShaderVertex
        /// </summary>
        /// <param name="position">The position in space for this vertex</param>
        /// <param name="normal">The normal for this vector</param>
        /// <param name="texCoords">The UV co-ords for this vertex</param>
        /// <param name="color">The color of this vertex</param>
        /// <param name="blockLight">The blockLight value for this vertex</param>
        /// <param name="skyLight">The skyLight value for this vertex</param>
        public MinifiedBlockShaderVertex(Vector3 position, BlockFace normal, Vector4 texCoords, Color color, byte blockLight, byte skyLight)
        {
	        Position = position;
	        TexCoords = texCoords;
	        Color = color;
	        Lighting = new Short2(skyLight, blockLight);
	        Normal = (byte)normal;
        }

        /// <summary>
        ///     The vertex declaration for this vertex type
        /// </summary>
        public static VertexDeclaration VertexDeclaration { get; } = new VertexDeclaration
        (
	        new VertexElement(0, VertexElementFormat.Vector3,VertexElementUsage.Position, 0),
	        new VertexElement(3 * sizeof(float), VertexElementFormat.Single,VertexElementUsage.Normal, 0),
	        new VertexElement(4 * sizeof(float), VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
	        new VertexElement((8 * sizeof(float)), VertexElementFormat.Color, VertexElementUsage.Color, 0),
	        new VertexElement((8 * sizeof(float)) + (4 * sizeof(byte)), VertexElementFormat.Short2, VertexElementUsage.TextureCoordinate, 1)
        );
        
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
	
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

        
        //public short BlockLight;

        //public short SkyLight;
        public Short2 Lighting;
        
        public BlockFace Face;
        
        /// <summary>
        ///     Creates a new VertexPositionNormalTextureColor
        /// </summary>
        /// <param name="position">The position in space for this vertex</param>
        /// <param name="normal">The nomal for this vector</param>
        /// <param name="texCoords">The UV co-ords for this vertex</param>
        public BlockShaderVertex(Vector3 position, Vector3 normal, Vector2 texCoords) : this(position, normal, texCoords, Microsoft.Xna.Framework.Color.White)
        {
            
        }

        //public Vector3 Normal;

        /// <summary>
        ///     Creates a new VertexPositionNormalTextureColor
        /// </summary>
        /// <param name="position">The position in space for this vertex</param>
        /// <param name="normal">The nomal for this vector</param>
        /// <param name="texCoords">The UV co-ords for this vertex</param>
        /// <param name="color">The color of this vertex</param>
        public BlockShaderVertex(Vector3 position, Vector3 normal, Vector2 texCoords, Color color) : this(position, normal, texCoords, color, 0, 15)
        {
            
        }

        /// <summary>
        ///     Creates a new VertexPositionNormalTextureColor
        /// </summary>
        /// <param name="position">The position in space for this vertex</param>
        /// <param name="normal">The nomal for this vector</param>
        /// <param name="texCoords">The UV co-ords for this vertex</param>
        /// <param name="color">The color of this vertex</param>
        public BlockShaderVertex(Vector3 position, Vector3 normal, Vector2 texCoords, Color color, byte blockLight, byte skyLight)
        {
	        Position = position;
	        //Normal = normal;
	        TexCoords = texCoords;
	        Color = color;
	        Lighting = new Short2(skyLight, blockLight);
	        Face = BlockFace.None;
        }
        
        /// <summary>
        ///     The vertex declaration for this vertex type
        /// </summary>
        public static VertexDeclaration VertexDeclaration { get; } = new VertexDeclaration
        (
	        new VertexElement(0, VertexElementFormat.Vector3,
		        VertexElementUsage.Position, 0),
	        new VertexElement((3 * sizeof(float)), VertexElementFormat.Vector2,
		        VertexElementUsage.TextureCoordinate, 0),
	        new VertexElement((5 * sizeof(float)), VertexElementFormat.Color,
		        VertexElementUsage.Color, 0),
	        new VertexElement((5 * sizeof(float)) + (4 * sizeof(byte)), VertexElementFormat.Short2, VertexElementUsage.TextureCoordinate, 1),
	        new VertexElement((5 * sizeof(float)) + (4 * sizeof(byte)) + (2 * sizeof(short)), VertexElementFormat.Single, VertexElementUsage.Position,1)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}
