using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.Graphics.Models.Items
{
    public class ItemModelRenderer : Model, IAttachable
    {
	    public ResourcePackItem Model { get; }
	    public VertexPositionColor[] Vertices { get; set; } = null;
		public short[] Indexes { get; set; } = null;

		private BasicEffect Effect { get; set; } = null;
		public Vector3 Offset { get; set; } = Vector3.Zero;
		public Vector3 Rotation { get; set; } = Vector3.Zero;
		public Vector3 Translation { get; set; }= Vector3.Zero;
		public Vector3 Scale { get; set; }= Vector3.Zero;
		public ItemModelRenderer(ResourcePackItem model, McResourcePack resourcePack)
		{
			Model = model;
			Cache(resourcePack);
		}

		private float Yaw { get; set; } = 0;
		public void Update(Vector3 attachmentPoint, float positionYaw)
		{
			Yaw = positionYaw;
			Offset = attachmentPoint;
			//Effect.World = World *
			//               Microsoft.Xna.Framework.Matrix.CreateTranslation(attachmentPoint);
		}

		public void Render(IRenderArgs args)
		{
			Render(args.GraphicsDevice);
		}

		public void Update(GraphicsDevice device, ICamera camera)
		{
			if (Effect == null)
			{
				Effect = new BasicEffect(device);
				Effect.VertexColorEnabled = true;
			}

			Effect.Projection = camera.ProjectionMatrix;
			Effect.View = camera.ViewMatrix;
			/*Effect.World = Matrix.CreateRotationX(MathUtils.ToRadians(Rotation.X)) *
				Matrix.CreateRotationY(MathUtils.ToRadians(Rotation.Y)) *
				Matrix.CreateRotationZ(MathUtils.ToRadians(Rotation.Z)) *
				Matrix.CreateScale(Scale) * (Matrix.CreateTranslation(-(Translation * (1/16f))) * Matrix.CreateRotationY(MathUtils.ToRadians(-(Yaw))) * Matrix.CreateTranslation((Translation * (1/16f)))) * (Matrix.CreateTranslation(camera.Position + ((Offset + Translation) * (1/16f))));
			*/
			
			//Effect.World = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Translation * (1f/16f)) * Matrix.CreateRotationY(MathUtils.ToRadians((270f - Yaw))) * (Matrix.CreateTranslation(camera.Position + ((Offset) * (1/16f))));
			//Effect.World = Matrix.CreateTranslation(Translation) *
			//               Matrix.CreateRotationY(MathUtils.ToRadians(270f - Yaw)) * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(camera.Position + Offset);

			Matrix characterMatrix = 
				Matrix.CreateRotationY(MathUtils.ToRadians(-Yaw)) *
			                         Matrix.CreateTranslation(camera.Position);

			var pieceMatrix =
				Matrix.CreateScale(Scale) *
						Matrix.CreateRotationX(Rotation.X) *
						Matrix.CreateRotationY(Rotation.Y) *
						Matrix.CreateRotationZ(Rotation.Z) *
			            Matrix.CreateTranslation((Translation * (1f/16f) + Offset * (1f/16f)));
			
			Effect.World = pieceMatrix * characterMatrix;
		}
		
		public void Render(GraphicsDevice device)
		{
			if (Effect == null || Vertices == null || Vertices.Length == 0)
				return;
			
			//Effect.World = Microsoft.Xna.Framework.Matrix.CreateScale(1f/16f) * Microsoft.Xna.Framework.Matrix.CreateTranslation(renderPosition);

			foreach (var a in Effect.CurrentTechnique.Passes)
			{
				a.Apply();
				device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indexes, 0, Indexes.Length / 3);
			}
		}
		
	    private void Cache(McResourcePack pack)
	    {
		    var t = Model.Textures.FirstOrDefault(x => x.Value != null);

		    if (t.Value == default) return;
		    
		    List<VertexPositionColor> vertices = new List<VertexPositionColor>(); 
		    List<short> indexes = new List<short>();
		    
		    if (pack.TryGetBitmap(t.Value, out Bitmap texture))
		    {
			    int i = 0;
			    float toolPosX = 0.0f;
			    float toolPosY = 0.0f;
			    float toolPosZ = 0.0f;
			    int verticesPerTool = texture.Width * texture.Height * 36;
			    
				    
			    for (int y = 0; y < texture.Height; y++)
			    {
				    for (int x = 0; x < texture.Width; x++)
				    {
					    var pixel = texture.GetPixel(x, y);
					    if (pixel.A == 0)
					    {
						    continue;
					    }

					    Color color = new Color(pixel.R, pixel.G, pixel.B, pixel.A);

					    ItemModelCube built = new ItemModelCube(new Vector3(1f / texture.Width));
					    built.BuildCube(color);

					    var origin = new Vector3(toolPosX + (1f / texture.Width) * x, toolPosY - (1f / texture.Height) * y, toolPosZ);
						
					    vertices = ModifyCubeIndexes(vertices, ref built.Front, origin);
					    vertices = ModifyCubeIndexes(vertices, ref built.Back, origin);
					    vertices = ModifyCubeIndexes(vertices, ref built.Top, origin);
					    vertices = ModifyCubeIndexes(vertices, ref built.Bottom, origin);
					    vertices = ModifyCubeIndexes(vertices, ref built.Left, origin);
					    vertices = ModifyCubeIndexes(vertices, ref built.Right, origin);

					    var indices = built.Front.indexes
						    .Concat(built.Back.indexes)
						    .Concat(built.Top.indexes)
						    .Concat(built.Bottom.indexes)
						    .Concat(built.Left.indexes)
						    .Concat(built.Right.indexes)
						    .ToArray();

					    indexes.AddRange(indices);
				    }
			    }
		    }

		    Vertices = vertices.ToArray();

		    for (var index = 0; index < Vertices.Length; index++)
		    {
			    var vertice = Vertices[index];

			   /* vertice.Position = Vector3.Transform(vertice.Position,
				    Matrix.CreateRotationX(MathUtils.ToRadians(Rotation.X)) *
				    Matrix.CreateRotationY(MathUtils.ToRadians(Rotation.Y)) *
				    Matrix.CreateRotationZ(MathUtils.ToRadians(Rotation.Z)));
*/
			    Vertices[index] = vertice;
		    }

		    Indexes = indexes.ToArray();

		    // int verticesPerTool = TOOL_TEXTURE_SIZE * TOOL_TEXTURE_SIZE * 36;
	    }
	    
	    private List<VertexPositionColor> ModifyCubeIndexes(List<VertexPositionColor> vertices,
		    ref (VertexPositionColor[] vertices, short[] indexes) data, Vector3 offset)
	    {
		    var startIndex = (short)vertices.Count;
		    foreach (var vertice in data.vertices)
		    {
			    var vertex = vertice;
			    vertex.Position += offset;
			    vertices.Add(vertex);
		    }
			
		    //vertices.AddRange(data.vertices);
			
		    for (int i = 0; i < data.indexes.Length; i++)
		    {
			    data.indexes[i] += startIndex;
		    }

		    return vertices;
	    }
	    
	     protected VertexPositionColor[] GetFaceVertices(BlockFace blockFace, Vector3 startPosition, Vector3 endPosition, Color faceColor, out int[] indexes)
		{
			var size = (endPosition - startPosition);
			
			Vector3 normal = Vector3.Zero;
			Vector3 positionTopLeft = Vector3.Zero, positionBottomLeft = Vector3.Zero, positionBottomRight = Vector3.Zero, positionTopRight = Vector3.Zero;

			switch (blockFace)
			{
				case BlockFace.Up: //Positive Y
					positionTopLeft = From(startPosition, endPosition, endPosition);
					positionTopRight = From(endPosition, endPosition, endPosition);

					positionBottomLeft = From(startPosition, endPosition, startPosition);
					positionBottomRight = From(endPosition, endPosition, startPosition);

					normal = Vector3.Up;
					break;
				case BlockFace.Down: //Negative Y
					positionTopLeft = From(startPosition, startPosition, endPosition);
					positionTopRight = From(endPosition, startPosition, endPosition);

					positionBottomLeft = From(startPosition, startPosition, startPosition);
					positionBottomRight = From(endPosition, startPosition, startPosition);

					normal = Vector3.Down;
					break;
				case BlockFace.West: //Negative X
					positionTopLeft = From(startPosition, endPosition, startPosition);
					positionTopRight = From(startPosition, endPosition, endPosition);

					positionBottomLeft = From(startPosition, startPosition, startPosition);
					positionBottomRight = From(startPosition, startPosition, endPosition);

					normal = Vector3.Left;
					break;
				case BlockFace.East: //Positive X
					positionTopLeft = From(endPosition, endPosition, startPosition);
					positionTopRight = From(endPosition, endPosition, endPosition);

					positionBottomLeft = From(endPosition, startPosition, startPosition);
					positionBottomRight = From(endPosition, startPosition, endPosition);

					normal = Vector3.Right;
					break;
				case BlockFace.South: //Positive Z
					positionTopLeft = From(startPosition, endPosition, startPosition);
					positionTopRight = From(endPosition, endPosition, startPosition);

					positionBottomLeft = From(startPosition, startPosition, startPosition);
					positionBottomRight = From(endPosition, startPosition, startPosition);

					normal = Vector3.Backward;
					break;
				case BlockFace.North: //Negative Z
					positionTopLeft = From(startPosition, endPosition, endPosition);
					positionTopRight = From(endPosition, endPosition, endPosition);

					positionBottomLeft = From(startPosition, startPosition, endPosition);
					positionBottomRight = From(endPosition, startPosition, endPosition);

					normal = Vector3.Forward;
					break;
				case BlockFace.None:
					break;
			}

			var topLeft = new VertexPositionColor(positionTopLeft, faceColor);
			var topRight = new VertexPositionColor(positionTopRight, faceColor);
			var bottomLeft = new VertexPositionColor(positionBottomLeft, faceColor);
			var bottomRight = new VertexPositionColor(positionBottomRight, faceColor);

			switch (blockFace)
			{
				case BlockFace.Up:
					indexes = new int[]
					{
						2, 0, 1,
						3, 2, 1
					};
					break;
				case BlockFace.Down:
					indexes = new[]
					{
						0, 2, 1,
						2, 3, 1
					};
					break;
				case BlockFace.North:
					indexes = new[]
					{
						0, 2, 1,
						2, 3, 1
					};
					break;
				case BlockFace.East:
					indexes = new[]
					{
						2, 0, 1,
						3, 2, 1
					};
					break;
				case BlockFace.South:
					indexes = new[]
					{
						2, 0, 1,
						3, 2, 1
					};
					break;
				case BlockFace.West:
					indexes = new[]
					{
						0, 2, 1,
						2, 3, 1
					};
					break;
				default:
					indexes = new int[0];
					break;
			}
			
			return new[]
			{
				topLeft, topRight, bottomLeft, bottomRight
			};

			return new VertexPositionColor[0];
		}
	     
	     private static Vector3 From(Vector3 x, Vector3 y, Vector3 z)
	     {
		     return new Vector3(x.X, y.Y, z.Z);
	     }
	     
    }

    public sealed class ItemModelCube
    {
	    public Vector3 Size;
	    
	    public bool Mirrored { get; set; } = false;

	    public ItemModelCube(Vector3 size)
	    {
		    this.Size = size;

		    //front verts with position and texture stuff
		    _topLeftFront = new Vector3(0.0f, 1.0f, 0.0f) * Size;
		    _topLeftBack = new Vector3(0.0f, 1.0f, 1.0f) * Size;
		    _topRightFront = new Vector3(1.0f, 1.0f, 0.0f) * Size;
		    _topRightBack = new Vector3(1.0f, 1.0f, 1.0f) * Size;

		    // Calculate the position of the vertices on the bottom face.
		    _btmLeftFront = new Vector3(0.0f, 0.0f, 0.0f) * Size;
		    _btmLeftBack = new Vector3(0.0f, 0.0f, 1.0f) * Size;
		    _btmRightFront = new Vector3(1.0f, 0.0f, 0.0f) * Size;
		    _btmRightBack = new Vector3(1.0f, 0.0f, 1.0f) * Size;
	    }

	    public (VertexPositionColor[] vertices, short[] indexes) Front, Back, Left, Right, Top, Bottom;

	    private readonly Vector3 _topLeftFront;
	    private readonly Vector3 _topLeftBack;
	    private readonly Vector3 _topRightFront;
	    private readonly Vector3 _topRightBack;
	    private readonly Vector3 _btmLeftFront;
	    private readonly Vector3 _btmLeftBack;
	    private readonly Vector3 _btmRightFront;
	    private readonly Vector3 _btmRightBack;

	    public void BuildCube(Color uv)
	    {
		    Front = GetFrontVertex(uv);
		    Back = GetBackVertex(uv);
		    Left = GetLeftVertex(uv);
		    Right = GetRightVertex(uv);
		    Top = GetTopVertex(uv);
		    Bottom = GetBottomVertex(uv);
	    }
	    
	    private (VertexPositionColor[] vertices, short[] indexes) GetLeftVertex(Color color)
	    {
		    // Add the vertices for the RIGHT face. 
		    return (new VertexPositionColor[]
		    {
			    new VertexPositionColor(_topLeftFront, color),
			    new VertexPositionColor(_btmLeftFront, color),
			    new VertexPositionColor(_btmLeftBack, color),
			    new VertexPositionColor(_topLeftBack, color),
			    //new VertexPositionNormalTexture(_topLeftFront , normal, map.TopLeft),
			    //new VertexPositionNormalTexture(_btmLeftBack, normal, map.BotRight),
		    }, new short[]
		    {
			    0, 1, 2,
			    3, 0, 2
			    //0, 1, 2, 3, 0, 2
		    });
	    }

	    private (VertexPositionColor[] vertices, short[] indexes) GetRightVertex(Color color)
	    {
		    // Add the vertices for the RIGHT face. 
		    return (new VertexPositionColor[]
		    {
			    new VertexPositionColor(_topRightFront, color),
			    new VertexPositionColor(_btmRightBack, color),
			    new VertexPositionColor(_btmRightFront, color),
			    new VertexPositionColor(_topRightBack, color),
			    //new VertexPositionNormalTexture(_btmRightBack , normal, map.BotLeft),
			    //new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
		    }, new short[]
		    {
			    0, 1, 2,
			    3, 1, 0
		    });
	    }

	    private (VertexPositionColor[] vertices, short[] indexes) GetFrontVertex(Color color)
	    {
		    // Add the vertices for the RIGHT face. 
		    return (new VertexPositionColor[]
		    {
			    new VertexPositionColor(_topLeftFront, color),
			    new VertexPositionColor(_topRightFront, color),
			    new VertexPositionColor(_btmLeftFront, color),
			    //new VertexPositionNormalTexture(_btmLeftFront , color),
			    //new VertexPositionNormalTexture(_topRightFront, color),
			    new VertexPositionColor(_btmRightFront, color),
		    }, new short[]
		    {
			    0, 1, 2,
			    2, 1, 3
			    //0, 2, 1, 2, 3, 1
		    });
	    }

	    private (VertexPositionColor[] vertices, short[] indexes) GetBackVertex(Color color)
	    {
		    // Add the vertices for the RIGHT face. 
		    return (new VertexPositionColor[]
		    {
			    new VertexPositionColor(_topLeftBack, color),
			    new VertexPositionColor(_btmLeftBack, color),
			    new VertexPositionColor(_topRightBack, color),
			    //new VertexPositionNormalTexture(_btmLeftBack , color),
			    new VertexPositionColor(_btmRightBack, color),
			    //new VertexPositionNormalTexture(_topRightBack, color),
		    }, new short[]
		    {
			    0, 1, 2,
			    1, 3, 2
			    //0, 1, 2, 1, 3, 2
		    });
	    }

	    private (VertexPositionColor[] vertices, short[] indexes) GetTopVertex(Color color)
	    {
		    // Add the vertices for the RIGHT face. 
		    return (new VertexPositionColor[]
		    {
			    new VertexPositionColor(_topLeftFront, color),
			    new VertexPositionColor(_topLeftBack, color),
			    new VertexPositionColor(_topRightBack, color),
			    //new VertexPositionNormalTexture(_topLeftFront , color),
			    //	new VertexPositionNormalTexture(_topRightBack , color),
			    new VertexPositionColor(_topRightFront, color),
		    }, new short[]
		    {
			    0, 1, 2,
			    0, 2, 3
		    });
	    }

	    private (VertexPositionColor[] vertices, short[] indexes) GetBottomVertex(Color color)
	    {
		    // Add the vertices for the RIGHT face. 
		    return (new VertexPositionColor[]
		    {
			    new VertexPositionColor(_btmLeftFront, color),
			    new VertexPositionColor(_btmRightBack, color),
			    new VertexPositionColor(_btmLeftBack, color),
			    //new VertexPositionNormalTexture(_btmLeftFront , color),
			    new VertexPositionColor(_btmRightFront, color),
			    //new VertexPositionNormalTexture(_btmRightBack , color),
		    }, new short[]
		    {
			    0, 1, 2,
			    0, 3, 1
		    });
	    }
    }
}
