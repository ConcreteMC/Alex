using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Alex.Blocks;
using Alex.Rendering;
using Alex.Utils;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using ResourcePackLib.Json;
using ResourcePackLib.Json.BlockStates;
using ResourcePackLib.Json.Models;
using Axis = ResourcePackLib.Json.Axis;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using V3 = Microsoft.Xna.Framework.Vector3;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Alex.Graphics.Models
{
    public class ResourcePackModel : Model
	{
        private BlockStateModel Variant { get; }
		private ResourceManager Resources { get; }

		public ResourcePackModel(ResourceManager resources, BlockStateModel variant)
		{
			Resources = resources;
            Variant = variant;
        }

		public override VertexPositionNormalTextureColor[] GetShape(World world, V3 position, Block baseBlock)
        {
	        var verts = new List<VertexPositionNormalTextureColor>();

	        var modelRotationMatrix =
		        Matrix.CreateRotationX((float) MathUtils.ToRadians(360f - Variant.X)) *
		        Matrix.CreateRotationY((float) MathUtils.ToRadians(360f - Variant.Y));

			// MaxY = 0;
	        V3 worldPosition = new V3(position.X, position.Y, position.Z);

			foreach (var element in Variant.Model.Elements)
            {
	            Matrix elementRotationMatrix = modelRotationMatrix;

	            var c = new V3(8f, 8f, 8f);
	            var rotMatrix = Matrix.CreateTranslation(-c) * elementRotationMatrix *
	                            Matrix.CreateTranslation(c);

				var elementFrom = new V3((element.From.X), (element.From.Y),
		           (element.From.Z));

	            var elementTo = new V3((element.To.X), (element.To.Y) ,
		            (element.To.Z));

				foreach (var face in element.Faces)
				{
					var startPosition = elementFrom;// V3.Transform(elementFrom, rotMatrix);
					var endPosition = elementTo;// V3.Transform(elementTo, rotMatrix);

					float x1 = 0, x2 = 1 / 32f, y1 = 0, y2 = 1 / 32f;
					if (Resources != null)
					{
						string textureName = "";
						if (!Variant.Model.TextureDefinitions.TryGetValue(face.Value.TextureName.Replace("#", ""), out textureName))
						{
							textureName = face.Value.TextureName;
						}

						if (textureName.StartsWith("#"))
						{
							if (!Variant.Model.TextureDefinitions.TryGetValue(textureName.Replace("#", ""), out textureName))
							{
								textureName = "no_texture";
							}
						}

						//Debug.WriteLine($"Texture: {textureName}");
						var textureLocation = Resources.Atlas.GetAtlasLocation(textureName.Replace("blocks/", ""));

						var tileSizeX = (1f / Resources.Atlas.InWidth) / Resources.Atlas.TextureWidth; //0.0625
						var tileSizeY = (1f / Resources.Atlas.InHeigth) / Resources.Atlas.TextureHeight;

						var uv = face.Value.UV;

						var uvSize = Resources.Atlas.AtlasSize;
						textureLocation.X /= uvSize.X;
						textureLocation.Y /= uvSize.Y;

						x1 = textureLocation.X + (uv.X1 * (Resources.Atlas.TextureWidth / 16) * tileSizeX);
						x2 = textureLocation.X + (uv.X2 * (Resources.Atlas.TextureWidth / 16) * tileSizeX);
						y1 = textureLocation.Y + (uv.Y1 * (Resources.Atlas.TextureHeight / 16) * tileSizeY);
						y2 = textureLocation.Y + (uv.Y2 * (Resources.Atlas.TextureHeight / 16) * tileSizeY);
					}

					var uvmap = new UVMap(new Microsoft.Xna.Framework.Vector2(x1, y1),
						new Microsoft.Xna.Framework.Vector2(x2, y1), new Microsoft.Xna.Framework.Vector2(x1, y2),
						new Microsoft.Xna.Framework.Vector2(x2, y2), baseBlock.SideColor, baseBlock.TopColor, baseBlock.BottomColor);

					V3 cullFace = V3.Zero;

					BlockFace cull;
					if (!Enum.TryParse(face.Value.CullFace, out cull))
					{
						cull = face.Key;
					}
					switch (cull)
					{
						case BlockFace.Up:
							cullFace = V3.Up;
							break;
						case BlockFace.Down:
							cullFace = V3.Down;
							break;
						case BlockFace.North:
							cullFace = V3.Backward;
							break;
						case BlockFace.South:
							cullFace = V3.Forward;
							break;
						case BlockFace.West:
							cullFace = V3.Left;
							break;
						case BlockFace.East:
							cullFace = V3.Right;
							break;
					}

					cullFace = V3.Transform(cullFace, elementRotationMatrix);

					if (cullFace != V3.Zero && !CanRender(world, baseBlock, worldPosition + cullFace))
						continue;

					var rotation = element.Rotation;



					VertexPositionNormalTextureColor[] faceVertices = GetFaceVertices(world, worldPosition + cullFace, face.Value, face.Key, startPosition, endPosition, uvmap, element.Shade);
					for (var index = 0; index < faceVertices.Length; index++)
					{
						var vert = faceVertices[index];
						var originalPos = new V3(vert.Position.X, vert.Position.Y, vert.Position.Z);						

						if (rotation.Axis != Axis.Undefined)
						{
							var rotationOrigin = new V3(rotation.Origin.X, rotation.Origin.Y, rotation.Origin.Z);
							//rotationOrigin += elementFrom;

							var angle = MathUtils.ToRadians((float)(rotation.Axis == Axis.X ? -rotation.Angle : rotation.Angle));
							angle = rotation.Axis == Axis.Z ? angle : -angle;
							var ci = 1f / (float)Math.Cos(angle);

							Matrix faceRotation = Matrix.CreateTranslation(-rotationOrigin);
							//offset += rotationOrigin;
							if (rotation.Axis == ResourcePackLib.Json.Axis.X)
							{
								faceRotation *= Matrix.CreateRotationX(angle);
							}
							else if (rotation.Axis == ResourcePackLib.Json.Axis.Y)
							{
								faceRotation *= Matrix.CreateRotationY(angle);
							}
							else if (rotation.Axis == ResourcePackLib.Json.Axis.Z)
							{
								faceRotation *= Matrix.CreateRotationZ(angle);
							}

							faceRotation *= Matrix.CreateTranslation(rotationOrigin);

							vert.Position = V3.Transform(vert.Position, faceRotation);

							if (rotation.Rescale)
							{
								if (rotation.Axis == Axis.X || rotation.Axis == Axis.Z)
								{
									vert.Position.Y *= ci;
								}

								if (rotation.Axis == Axis.Y || rotation.Axis == Axis.Z)
								{
									vert.Position.X *= ci;
								}

								if (rotation.Axis == Axis.Y || rotation.Axis == Axis.X)
								{
									vert.Position.Z *= ci;
								}
							}
							
						}
						
						vert.Position = V3.Transform(vert.Position, rotMatrix);

						vert.Position = position + (vert.Position / 16f);

						verts.Add(vert);
					}
				}
			}

	        return verts.ToArray();
        }

		private VertexPositionNormalTextureColor[] GetFaceVertices(World world, V3 pos, BlockModelElementFace face, BlockFace blockFace, V3 startPosition, V3 endPosition, UVMap uvmap, bool shade)
		{
			List< VertexPositionNormalTextureColor > verts = new List<VertexPositionNormalTextureColor>();

			Color faceColor = Color.White;
			V3 normal = V3.Zero;
			V3 textureTopLeft = V3.Zero, textureBottomLeft = V3.Zero, textureBottomRight = V3.Zero, textureTopRight = V3.Zero;
			switch (blockFace)
			{
				case BlockFace.Up:
					textureTopLeft = VectorExtension.From(startPosition, endPosition, endPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, endPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, endPosition, startPosition);

					normal = V3.Up;
					faceColor = uvmap.ColorTop; //new Color(0x00, 0x00, 0xFF);
					break;
				case BlockFace.Down:
					textureTopLeft = VectorExtension.From(startPosition, startPosition, endPosition);
					textureTopRight = VectorExtension.From(endPosition, startPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, startPosition);

					normal = V3.Down;
					faceColor = uvmap.ColorBottom; //new Color(0xFF, 0xFF, 0x00);
					break;
				case BlockFace.West: //Left side
					textureTopLeft = VectorExtension.From(startPosition, endPosition, startPosition);
					textureTopRight = VectorExtension.From(startPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(startPosition, startPosition, endPosition);

					normal = V3.Left;
					faceColor = uvmap.ColorLeft; // new Color(0xFF, 0x00, 0xFF);
					break;
				case BlockFace.East: //Right side
					textureTopLeft = VectorExtension.From(endPosition, endPosition, startPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(endPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, endPosition);

					normal = V3.Right;
					faceColor = uvmap.ColorRight; //new Color(0x00, 0xFF, 0xFF);
					break;
				case BlockFace.South: //Front
					textureTopLeft = VectorExtension.From(startPosition, endPosition, startPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, startPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, startPosition);

					normal = V3.Forward;
					faceColor = uvmap.ColorFront; // ew Color(0x00, 0xFF, 0x00);
					break;
				case BlockFace.North: //Back
					textureTopLeft = VectorExtension.From(startPosition, endPosition, endPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, endPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, endPosition);

					normal = V3.Backward;
					faceColor = uvmap.ColorBack; // new Color(0xFF, 0x00, 0x00);
					break;
				case BlockFace.None:
					break;
			}

			if (shade)
			{
				faceColor = UvMapHelp.AdjustColor(faceColor, blockFace, GetLight(world, pos));
			}

			Microsoft.Xna.Framework.Vector2 uvTopLeft = uvmap.TopLeft;
			Microsoft.Xna.Framework.Vector2 uvTopRight = uvmap.TopRight;
			Microsoft.Xna.Framework.Vector2 uvBottomLeft = uvmap.BottomLeft;
			Microsoft.Xna.Framework.Vector2 uvBottomRight = uvmap.BottomRight;

			var faceRotation = face.Rotation;
			if (!Variant.Uvlock)
			{
				if (faceRotation == 90)
				{
					uvTopLeft = uvmap.BottomLeft;
					uvTopRight = uvmap.TopLeft;

					uvBottomLeft = uvmap.BottomRight;
					uvBottomRight = uvmap.TopRight;
				}
				else if (faceRotation == 180)
				{
					uvTopLeft = uvmap.BottomRight;
					uvTopRight = uvmap.BottomLeft;

					uvBottomLeft = uvmap.TopRight;
					uvBottomRight = uvmap.TopLeft;
				}
				else if (faceRotation == 270)
				{
					uvTopLeft = uvmap.BottomLeft;
					uvTopRight = uvmap.TopLeft;

					uvBottomLeft = uvmap.BottomRight;
					uvBottomRight = uvmap.TopRight;
				}
			}

			var topLeft = new VertexPositionNormalTextureColor(textureTopLeft, normal, uvTopLeft, faceColor);
			var topRight = new VertexPositionNormalTextureColor(textureTopRight, normal, uvTopRight, faceColor);
			var bottomLeft = new VertexPositionNormalTextureColor(textureBottomLeft, normal, uvBottomLeft,
				faceColor);
			var bottomRight = new VertexPositionNormalTextureColor(textureBottomRight, normal, uvBottomRight,
				faceColor);

			switch (blockFace)
			{
				case BlockFace.Up:
					verts.AddRange(new[]
					{
								bottomLeft, topLeft, topRight,
								bottomRight, bottomLeft, topRight
							});
					break;
				case BlockFace.Down:
					verts.AddRange(new[]
					{
								topLeft, bottomLeft, topRight,
								bottomLeft, bottomRight, topRight
							});
					break;
				case BlockFace.North:
					verts.AddRange(new[]
					{
								topLeft, bottomLeft, topRight,
								bottomLeft, bottomRight, topRight
							});
					break;
				case BlockFace.East:
					verts.AddRange(new[]
					{
								bottomLeft, topLeft, topRight,
								bottomRight, bottomLeft, topRight
							});
					break;
				case BlockFace.South:
					verts.AddRange(new[]
					{
								bottomLeft, topLeft, topRight,
								bottomRight, bottomLeft, topRight
							});
					break;
				case BlockFace.West:
					verts.AddRange(new[]
					{
								topLeft, bottomLeft, topRight,
								bottomLeft, bottomRight, topRight
							});
					break;
			}

			return verts.ToArray();
		}

		public override BoundingBox GetBoundingBox(V3 position, Block requestingBlock)
		{
			return new BoundingBox(position, position + new V3(1, 1, 1));
		}
	}

	public static class VectorExtension {
		public static V3 From(V3 x, V3 y, V3 z)
		{
			return new V3(x.X, y.Y, z.Z);
		}
	}
}
