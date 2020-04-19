using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Blocks.State;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.Blocks.Minecraft;
using Alex.Entities;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp.PixelFormats;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.Graphics.Models.Items
{
	public interface IItemRenderer : IAttachable
	{
		ResourcePackItem Model { get; }
		
		Vector3 Rotation { get; set; }
		Vector3 Translation { get; set; }
		Vector3 Scale { get; set; }
		
		Vector3 Position { get; set; }
		
		void Update(GraphicsDevice device, ICamera camera);

		void Cache(McResourcePack pack);
	}
	
	public class ItemBlockModelRenderer : ItemModelRenderer<VertexPositionNormalTextureColor>
	{
		private IBlockState _block;
		private ResourceManager _resource;
		public ItemBlockModelRenderer(IBlockState block, ResourcePackItem model, McResourcePack resourcePack, ResourceManager resourceManager) : base(model, resourcePack, VertexPositionNormalTextureColor.VertexDeclaration)
		{
			_block = block;
			_resource = resourceManager;
			
			Scale = new Vector3(0.25f, 0.25f, 0.25f);
		}

		public override void Cache(McResourcePack pack)
		{
			if (Vertices != null)
				return;
			
			var data = _block.Model.GetVertices(new ItemRenderingWorld(_block.Block), Vector3.Zero, _block.Block);
			Vertices = data.vertices;
			Indexes = data.indexes.Select(x => (short)x).ToArray();
		}

		public override void Update(GraphicsDevice device, ICamera camera)
		{
			if (Effect == null)
			{
				Effect = new BasicEffect(device);
				Effect.VertexColorEnabled = true;
				Effect.TextureEnabled = true;

				if (_block.Block.Animated)
				{
					Effect.Texture = _resource.Atlas.GetAtlas(0);
				}
				else
				{
					Effect.Texture = _resource.Atlas.GetStillAtlas();
				}
			}

			Effect.Projection = camera.ProjectionMatrix;
			Effect.View = camera.ViewMatrix;

			var scale = Scale;

			Effect.World = Matrix.CreateScale(scale) * ParentMatrix;
			
			base.Update(device, camera);
		}
	}

	public class ItemModelRenderer : ItemModelRenderer<VertexPositionColor>
	{
		public ItemModelRenderer(ResourcePackItem model, McResourcePack resourcePack) : base(model, resourcePack, VertexPositionColor.VertexDeclaration)
		{
			
		}

		public override void Update(GraphicsDevice device, ICamera camera)
		{
			if (Effect == null)
			{
				Effect = new BasicEffect(device);
				Effect.VertexColorEnabled = true;
			}

			Effect.Projection = camera.ProjectionMatrix;
			Effect.View = camera.ViewMatrix;

			var scale = Scale;
			var trans = Translation;

			// var rotationMatrix = Matrix.CreateFromAxisAngle(Vector3.Forward, MathUtils.ToRadians(360f - Rotation.Z));
			// rotationMatrix *= Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(Rotation.Y));
			// rotationMatrix *= Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(Rotation.X));

			// var pieceMatrix =
			// 	//	Matrix.CreateTranslation(-vec) *
			// 	rotationMatrix *
			// 	Matrix.CreateTranslation(new Vector3((trans.X - 0.5f) * scale.X, (trans.Y * scale.Y) - 0.5f, -(trans.Z * scale.Z)));// *
			// 	//Matrix.CreateTranslation(vec);
			//
			// 	//Matrix.CreateTranslation(-vec);
			//
			// 	//Matrix.CreateTranslation(vector);

			Effect.World = 
				Matrix.CreateTranslation(new Vector3(-trans.X, -trans.Y, trans.Z)) *
				Matrix.CreateScale(scale) * 
			               Matrix.CreateFromAxisAngle(Vector3.Forward, MathUtils.ToRadians(360f - Rotation.Z)) *
			               Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(Rotation.Y)) *
			               Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(Rotation.X)) *
			               Matrix.CreateTranslation(
				               new Vector3(trans.X, trans.Y, -trans.Z)) *
							Matrix.CreateTranslation(0f, 0.62f, 0f)
			               * ParentMatrix;

			// var origin = new Vector3(0.5f, 0.5f,0.5f);
			//
			//
			// Effect.World = //Matrix.CreateTranslation(-origin) *
			//                Matrix.CreateScale(scale) *
			//                Matrix.CreateFromAxisAngle(Vector3.Forward, MathUtils.ToRadians(360f - Rotation.Z)) *
			//                Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(Rotation.Y)) *
			//                Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(Rotation.X)) *
			//               // Matrix.CreateTranslation(origin) *
			//                Matrix.CreateTranslation(trans) *
			//                ParentMatrix;
			//
			base.Update(device, camera);
		}


		public override void Cache(McResourcePack pack)
		{
			var t = Model.Textures.FirstOrDefault(x => x.Value != null);

		    if (t.Value == default)
		    {
			    return;
		    }
		    
		    List<VertexPositionColor> vertices = new List<VertexPositionColor>(); 
		    List<short> indexes = new List<short>();
		    
		    if (pack.TryGetBitmap(t.Value, out var rawTexture))
		    {
			    var texture = rawTexture.CloneAs<Rgba32>();
			    
			    int i = 0;
			    float toolPosX = 0.0f;
			    float toolPosY = 0.0f;
			    float toolPosZ = 0.0f;
			    int verticesPerTool = texture.Width * texture.Height * 36;
			    
				    
			    for (int y = 0; y < texture.Height; y++)
			    {
				    for (int x = 0; x < texture.Width; x++)
				    {
					    var pixel = texture[x, y];
					    if (pixel.A == 0)
					    {
						    continue;
					    }

					    Color color = new Color(pixel.R, pixel.G, pixel.B, pixel.A);

					    ItemModelCube built = new ItemModelCube(new Vector3(1f / texture.Width));
					    built.BuildCube(color);

					    var origin = new Vector3((toolPosX + (1f / texture.Width) * x), toolPosY - (1f / texture.Height) * y, toolPosZ);
						
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
			    Vertices[index] = vertice;
		    }

		    Indexes = indexes.ToArray();
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
	}
	
    public class ItemModelRenderer<TVertice> : Model, IAttachable, IItemRenderer where TVertice : struct, IVertexType
    {
	    public ResourcePackItem Model { get; }
	    public TVertice[] Vertices { get; set; } = null;
		public short[] Indexes { get; set; } = null;

		protected BasicEffect Effect { get; set; } = null;

		public Vector3 Rotation { get; set; } = Vector3.Zero;
		public Vector3 Translation { get; set; }= Vector3.Zero;
		public Vector3 Scale { get; set; }= Vector3.Zero;
		public Vector3 Position { get; set; } = Vector3.Zero;

		private VertexBuffer Buffer { get; set; } = null;
		private IndexBuffer IndexBuffer { get; set; } = null;
		private VertexDeclaration _declaration;
		public ItemModelRenderer(ResourcePackItem model, McResourcePack resourcePack, VertexDeclaration declaration)
		{
			Model = model;
			_declaration = declaration;
		}

		protected Matrix ParentMatrix = Matrix.Identity;
		public void Update(Matrix parentMatrix)
		{
			ParentMatrix = parentMatrix;
		}

		public void Render(IRenderArgs args)
		{
			Render(args.GraphicsDevice);
		}

		private bool _canInit = true;
		public virtual void Update(GraphicsDevice device, ICamera camera)
		{
			if (Buffer == null && Vertices != null && Indexes != null && _canInit)
			{
				var vertices = Vertices;
				var indexes = Indexes;

				if (vertices.Length == 0 || indexes.Length == 0)
				{
					_canInit = false;
				}
				else
				{
					var buffer = GpuResourceManager.GetBuffer(this, device, _declaration,
						Vertices.Length, BufferUsage.WriteOnly);
					var indexBuffer = GpuResourceManager.GetIndexBuffer(this, device, IndexElementSize.SixteenBits,
						Indexes.Length, BufferUsage.WriteOnly);

					buffer.SetData(vertices);
					indexBuffer.SetData(indexes);

					Buffer = buffer;
					IndexBuffer = indexBuffer;
				}
			}
		}

		private void DrawLine(GraphicsDevice device, Vector3 start, Vector3 end, Color color)
		{
			var vertices = new[] { new VertexPositionColor(start, color),  new VertexPositionColor(end, color) };
			device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
		}
		
		public void Render(GraphicsDevice device)
		{
			if (Effect == null || Buffer == null || Buffer.VertexCount == 0)
				return;

			foreach (var a in Effect.CurrentTechnique.Passes)
			{
				a.Apply();

				//DrawLine(device, Vector3.Zero, Vector3.Up, Color.Green);
				//	DrawLine(device, Vector3.Zero, Vector3.Forward, Color.Blue);
				//	DrawLine(device, Vector3.Zero, Vector3.Right, Color.Red);

				device.Indices = IndexBuffer;
				device.SetVertexBuffer(Buffer);
				device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount / 3);
				//device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indexes, 0, Indexes.Length / 3);
			}
		}
		
	    public virtual void Cache(McResourcePack pack)
	    {
		    
		  
			//Buffer = GpuResourceManager.GetBuffer(this, )
	    }
    }

    public sealed class ItemModelCube : Model
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
		    Front = GetFrontVertex(AdjustColor(uv, BlockFace.North, 15));
		    Back = GetBackVertex(AdjustColor(uv, BlockFace.South, 15));
		    Left = GetLeftVertex(AdjustColor(uv, BlockFace.West, 15));
		    Right = GetRightVertex(AdjustColor(uv, BlockFace.East, 15));
		    Top = GetTopVertex(AdjustColor(uv, BlockFace.Up, 15));
		    Bottom = GetBottomVertex(AdjustColor(uv, BlockFace.Down, 15));
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
