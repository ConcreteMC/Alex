using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Graphics;
using Alex.Gamestates;
using Alex.Rendering.Camera;
using Alex.ResourcePackLib.Json.Models;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;

namespace Alex.Graphics.Models
{
    public class EntityModelRenderer
    {
	    private static readonly ILog Log = LogManager.GetLogger(typeof(EntityModelRenderer));
		private EntityModel Model { get; }
		private Texture2D Texture { get; }
		private AlphaTestEffect Effect { get; set; }
		public EntityModelRenderer(EntityModel model, Texture2D texture)
		{
			Model = model;
			Texture = texture;

			Cache();
		}

	    private VertexPositionTexture[] Vertices { get; set; } = null;
		private VertexBuffer Buffer { get; set; }
		private void Cache()
		{
			float x = 0, y = 0, z = 0;
			List<VertexPositionTexture> textures = new List<VertexPositionTexture>();
		    foreach (var bone in Model.Bones)
		    {
				if (bone == null) continue;
				if (bone.NeverRender) continue;

			    if (bone.Cubes != null)
			    {
				    foreach (var cube in bone.Cubes)
				    {
					    if (cube == null)
					    {
						    Log.Warn("Cube was null!");
						    continue;
					    }

					    if (cube.Uv == null)
					    {
						    Log.Warn("Cube.UV was null!");
						    continue;
					    }

					    if (cube.Origin == null)
					    {
						    Log.Warn("Cube.Origin was null!");
						    continue;
					    }

					    if (cube.Size == null)
					    {
						    Log.Warn("Cube.Size was null!");
						    continue;
					    }

					    var pixelSizeX = 1f / Texture.Width; //0.0625
					    var pixelSizeY = 1f / Texture.Height;

					    var x1 = cube.Uv.X * pixelSizeX;
					    var x2 = (cube.Uv.X + cube.Size.X) * pixelSizeX; // + ((cube.Size.X) * pixelSizeX);
					    var y1 = cube.Uv.Y * pixelSizeY;
					    var y2 = (cube.Uv.Y + cube.Size.Y) * pixelSizeY; // + ((cube.Size.Y) * pixelSizeY);

					    var size = new Vector3(cube.Size.X, cube.Size.Y, cube.Size.Z);
					    var origin = new Vector3(cube.Origin.X, cube.Origin.Y, cube.Origin.Z);
					    var built = BuildCube(
						    size,
						    origin, x1, x2, y1, y2);

					    textures.AddRange(built.Front);
					    textures.AddRange(built.Back);
					    textures.AddRange(built.Left);
					    textures.AddRange(built.Right);
					    textures.AddRange(built.Top);
					    textures.AddRange(built.Bottom);
				    }
			    }
		    }

		    Vertices = textures.ToArray();
		}

	    private Cube BuildCube(Vector3 size, Vector3 origin, float x1, float x2, float y1, float y2)
	    {
			Cube cube = new Cube(size);
			cube.BuildCube(x1, x2, y1, y2);

		    Mod(ref cube.Back, origin);
		    Mod(ref cube.Front, origin);
		    Mod(ref cube.Left, origin);
		    Mod(ref cube.Right, origin);
		    Mod(ref cube.Top, origin);
		    Mod(ref cube.Bottom, origin);

			return cube;
	    }

	    private void Mod(ref VertexPositionTexture[] data, Vector3 o)
	    {
		    for (int i = 0; i < data.Length; i++)
		    {
			    var pos = data[i].Position;

			    pos = new Vector3(o.X + pos.X, o.Y + pos.Y, o.Z + pos.Z);
			    //pos /= 16;
			    data[i].Position = pos;
		    }
	    }

	    private float _angle = 0f;
		public void Render(IRenderArgs args, Camera camera, Vector3 position)
	    {
		    if (Vertices == null || Vertices.Length == 0) return;

			if (Effect == null || Buffer == null)
		    {
			    Effect = new AlphaTestEffect(args.GraphicsDevice);
			    Effect.Texture = Texture;

				Buffer = new VertexBuffer(args.GraphicsDevice, 
				    VertexPositionTexture.VertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);
				Buffer.SetData(Vertices);
			}
			
		    Effect.Projection = camera.ProjectionMatrix;
		    Effect.View = camera.ViewMatrix;
		    Effect.World = Matrix.CreateScale(1f / 16f) * Matrix.CreateTranslation(position);
	//	    Effect.World = Matrix.CreateScale(1f / 16f) * Matrix.CreateRotationY(3 * _angle) * Matrix.CreateTranslation(position);

			args.GraphicsDevice.SetVertexBuffer(Buffer);

			foreach (var pass in Effect.CurrentTechnique.Passes)
		    {
			    pass.Apply();
			    args.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, Vertices.Length);
		    }

		    float dt = (float) args.GameTime.ElapsedGameTime.TotalSeconds;
		    _angle += 0.5f * dt;
		}

		private class Cube
		{
			public Vector3 Size;

			public int Triangles = 12;

			public Cube(Vector3 size)
			{
				this.Size = size;
			}

			public VertexPositionTexture[] Front, Back, Left, Right, Top, Bottom;

			public void BuildCube(float x1, float x2, float y1, float y2)
			{
				//vertex arrays for each side of the cube
				Front = new VertexPositionTexture[6];
				Back = new VertexPositionTexture[6];
				Left = new VertexPositionTexture[6];
				Right = new VertexPositionTexture[6];
				Top = new VertexPositionTexture[6];
				Bottom = new VertexPositionTexture[6];

				//has to be an easier way to do this.  this stuff sets the points of the cube 
				//in relation to its size.  tedious figuring.  
				Vector3 topLeftFront = new Vector3(0f, 1.0f, 1.0f) * Size;
				Vector3 topRightFront = new Vector3(1.0f, 1.0f, 1.0f) * Size;
				Vector3 topLeftBack = new Vector3(0f, 1.0f, 0f) * Size;
				Vector3 topRightBack = new Vector3(1.0f, 1.0f, 0f) * Size;

				Vector3 botLeftFront = new Vector3(0f, 0f, 1.0f) * Size;
				Vector3 botRightFront =new Vector3(1.0f, 0f, 1.0f) * Size;
				Vector3 botLeftBack = new Vector3(0f, 0f, 0f) * Size;
				Vector3 botRightBack = new Vector3(1.0f, 0f, 0f) * Size;

				//this is the texturing coords
				Vector2 tTopLeft = new Vector2(x1, y1);
				Vector2 tTopRight = new Vector2(x2, y1);
				Vector2 tBotLeft = new Vector2(x1, y2);
				Vector2 tBotRight = new Vector2(x2, y2);

				//front verts with position and texture stuff
				Front[0] = new VertexPositionTexture(topLeftFront,  tTopLeft);
				Front[1] = new VertexPositionTexture(botLeftFront,  tBotLeft);
				Front[2] = new VertexPositionTexture(topRightFront,  tTopRight);
				Front[3] = new VertexPositionTexture(botLeftFront, tBotLeft);
				Front[4] = new VertexPositionTexture(botRightFront,  tBotRight);
				Front[5] = new VertexPositionTexture(topRightFront,  tTopRight);

				//back verts with position and texture stuff
				Back[0] = new VertexPositionTexture(topRightBack,  tTopRight);
				Back[1] = new VertexPositionTexture(botRightBack,  tBotRight);
				Back[2] = new VertexPositionTexture(topLeftBack,  tTopLeft);
				Back[3] = new VertexPositionTexture(botRightBack, tBotRight);
				Back[4] = new VertexPositionTexture(botLeftBack, tBotLeft);
				Back[5] = new VertexPositionTexture(topLeftBack, tTopLeft);

				//top side verts with position/texture stuff
				Top[0] = new VertexPositionTexture(botLeftFront, tBotLeft);
				Top[1] = new VertexPositionTexture(botRightBack, tTopRight);
				Top[2] = new VertexPositionTexture(botLeftBack, tTopLeft);
				Top[3] = new VertexPositionTexture(botLeftFront, tBotLeft);
				Top[4] = new VertexPositionTexture(botRightFront, tBotRight);
				Top[5] = new VertexPositionTexture(botRightBack, tTopRight);

				//bottom side verts with position/texture stuff
				Bottom[0] = new VertexPositionTexture(topLeftFront, tTopLeft);
				Bottom[1] = new VertexPositionTexture(topRightBack, tBotRight);
				Bottom[2] = new VertexPositionTexture(topLeftBack, tBotLeft);
				Bottom[3] = new VertexPositionTexture(topLeftFront, tTopLeft);
				Bottom[4] = new VertexPositionTexture(topRightFront, tTopRight);
				Bottom[5] = new VertexPositionTexture(topRightBack, tBotRight);

				//left side verts with position/texture stuff
				Left[0] = new VertexPositionTexture(topLeftFront,  tTopRight);
				Left[1] = new VertexPositionTexture(botLeftBack,  tBotLeft);
				Left[2] = new VertexPositionTexture(botLeftFront,  tBotRight);
				Left[3] = new VertexPositionTexture(topLeftBack,  tTopLeft);
				Left[4] = new VertexPositionTexture(botLeftBack, tBotLeft);
				Left[5] = new VertexPositionTexture(topLeftFront, tTopRight);

				//right side verts with position/texture stuff
				Right[0] = new VertexPositionTexture(topRightBack, tTopLeft);
				Right[1] = new VertexPositionTexture(botRightFront, tBotRight);
				Right[2] = new VertexPositionTexture(botRightBack,tBotLeft);
				Right[3] = new VertexPositionTexture(topRightFront,  tTopRight);
				Right[4] = new VertexPositionTexture(botRightFront, tBotRight);
				Right[5] = new VertexPositionTexture(topRightBack,  tTopLeft);
			}
		}
	}
}
