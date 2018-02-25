using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks;
using Alex.Rendering;
using Alex.Utils;
using log4net;
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
		private static readonly ILog Log = LogManager.GetLogger(typeof(ResourcePackModel));
		
        protected BlockStateModel Variant { get; }
		protected ResourceManager Resources { get; }

		public ResourcePackModel(ResourceManager resources, BlockStateModel variant)
		{
			Resources = resources;
            Variant = variant;

			CalculateBoundingBox();
        }

		private void CalculateBoundingBox()
		{
			foreach (var element in Variant.Model.Elements)
			{
				var faceStart = new V3((element.From.X), (element.From.Y),
					(element.From.Z)) / 16f;

				var faceEnd = new V3((element.To.X), (element.To.Y),
					(element.To.Z)) / 16f;

				if (faceEnd.X > Max.X)
				{
					Max.X = faceEnd.X;
				}

				if (faceEnd.Y > Max.Y)
				{
					Max.Y = faceEnd.Y;
				}

				if (faceEnd.Z > Max.Z)
				{
					Max.Z = faceEnd.Z;
				}

				if (faceStart.X < Min.X)
				{
					Min.X = faceStart.X;
				}

				if (faceStart.Y < Min.Y)
				{
					Min.Y = faceStart.Y;
				}

				if (faceStart.Z < Min.Z)
				{
					Min.Z = faceStart.Z;
				}
			}
		}

		private V3 Min = V3.Zero;
		private V3 Max = V3.One / 16f;
		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, V3 position, Block baseBlock)
        {
	        var verts = new List<VertexPositionNormalTextureColor>();

	        var modelRotationMatrix =
		        Matrix.CreateRotationX((float) MathUtils.ToRadians(360f - Variant.X)) *
		        Matrix.CreateRotationY((float) MathUtils.ToRadians(360f - Variant.Y));

			// MaxY = 0;
	        V3 worldPosition = new V3(position.X, position.Y, position.Z);

			foreach (var element in Variant.Model.Elements)
            {
	            var c = new V3(8f, 8f, 8f);

				var elementFrom = new V3((element.From.X), (element.From.Y),
		           (element.From.Z));

	            var elementTo = new V3((element.To.X), (element.To.Y) ,
		            (element.To.Z));

	          //  Max = V3.Max(Max, elementTo);
	         //   Min = V3.Min(Min, elementFrom);

				var elementModelRotation = Matrix.CreateTranslation(-c) * modelRotationMatrix *
	                            Matrix.CreateTranslation(c);

				foreach (var face in element.Faces)
				{
					var faceStart = elementFrom;
					var faceEnd = elementTo;

					var faceWidth = faceEnd.X - faceStart.X;
					var faceHeight = faceEnd.Y - faceStart.Y;
					var faceDepth = faceEnd.Z - faceStart.Z;

					float x1 = 0, x2 = 1 / 32f, y1 = 0, y2 = 1 / 32f;
					if (Resources != null)
					{
						string textureName = "";
						if (!Variant.Model.Textures.TryGetValue(face.Value.Texture.Replace("#", ""), out textureName))
						{
							textureName = face.Value.Texture;
						}

						if (textureName.StartsWith("#"))
						{
							if (!Variant.Model.Textures.TryGetValue(textureName.Replace("#", ""), out textureName))
							{
								textureName = "no_texture";
							}
						}

						var textureInfo = Resources.Atlas.GetAtlasLocation(textureName.Replace("blocks/", ""));
						var textureLocation = textureInfo.Position;

						var uvSize = Resources.Atlas.AtlasSize;

						var pixelSizeX = (textureInfo.Width / uvSize.X) / 16f; //0.0625
						var pixelSizeY = (textureInfo.Height / uvSize.Y) / 16f;

						var uv = face.Value.UV;

						textureLocation.X /= uvSize.X;
						textureLocation.Y /= uvSize.Y;

						x1 = textureLocation.X + (uv.X1 * pixelSizeX);
						x2 = textureLocation.X + (uv.X2 * pixelSizeX);
						y1 = textureLocation.Y + (uv.Y1 * pixelSizeY);
						y2 = textureLocation.Y + (uv.Y2 * pixelSizeY);
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

					cullFace = V3.Transform(cullFace, modelRotationMatrix);

					if (cullFace != V3.Zero && !CanRender(world, baseBlock, worldPosition + cullFace))
						continue;

					Matrix elementRotationMatrix = Matrix.Identity;
					float ci = 0f;
					var elementRotation = element.Rotation;
					if (elementRotation.Axis != Axis.Undefined)
					{
						var elementRotationOrigin = new V3(elementRotation.Origin.X, elementRotation.Origin.Y, elementRotation.Origin.Z);

						var elementAngle =
							MathUtils.ToRadians((float) (elementRotation.Axis == Axis.X ? -elementRotation.Angle : elementRotation.Angle));
						elementAngle = elementRotation.Axis == Axis.Z ? elementAngle : -elementAngle;
						ci = 1f / (float) Math.Cos(elementAngle);

						elementRotationMatrix = Matrix.CreateTranslation(-elementRotationOrigin);
						if (elementRotation.Axis == ResourcePackLib.Json.Axis.X)
						{
							elementRotationMatrix *= Matrix.CreateRotationX(elementAngle);
						}
						else if (elementRotation.Axis == ResourcePackLib.Json.Axis.Y)
						{
							elementRotationMatrix *= Matrix.CreateRotationY(elementAngle);
						}
						else if (elementRotation.Axis == ResourcePackLib.Json.Axis.Z)
						{
							elementRotationMatrix *= Matrix.CreateRotationZ(elementAngle);
						}

						elementRotationMatrix *= Matrix.CreateTranslation(elementRotationOrigin);
					}

					VertexPositionNormalTextureColor[] faceVertices = GetFaceVertices(face.Value, face.Key, faceStart, faceEnd, uvmap);

					Color faceColor = faceVertices[0].Color;
					if (element.Shade)
					{
						faceColor = UvMapHelp.AdjustColor(faceColor, cull, GetLight(world, worldPosition + cullFace));
					}

					for (var index = 0; index < faceVertices.Length; index++)
					{
						var vert = faceVertices[index];
						vert.Color = faceColor;

						if (elementRotation.Axis != Axis.Undefined)
						{
							vert.Position = V3.Transform(vert.Position, elementRotationMatrix);

							if (elementRotation.Rescale)
							{
								if (elementRotation.Axis == Axis.X || elementRotation.Axis == Axis.Z)
								{
									vert.Position.Y *= ci;
								}

								if (elementRotation.Axis == Axis.Y || elementRotation.Axis == Axis.Z)
								{
									vert.Position.X *= ci;
								}

								if (elementRotation.Axis == Axis.Y || elementRotation.Axis == Axis.X)
								{
									vert.Position.Z *= ci;
								}
							}						
						}
						
						vert.Position = V3.Transform(vert.Position, elementModelRotation);

						vert.Position = worldPosition + (vert.Position / 16f);

						verts.Add(vert);
					}
				}
			}

	        return verts.ToArray();
        }

		protected VertexPositionNormalTextureColor[] GetFaceVertices(BlockModelElementFace face, BlockFace blockFace, V3 startPosition, V3 endPosition, UVMap uvmap)
		{
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

			Microsoft.Xna.Framework.Vector2 uvTopLeft = uvmap.TopLeft;
			Microsoft.Xna.Framework.Vector2 uvTopRight = uvmap.TopRight;
			Microsoft.Xna.Framework.Vector2 uvBottomLeft = uvmap.BottomLeft;
			Microsoft.Xna.Framework.Vector2 uvBottomRight = uvmap.BottomRight;

			var faceRotation = face.Rotation;

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


			var topLeft = new VertexPositionNormalTextureColor(textureTopLeft, normal, uvTopLeft, faceColor);
			var topRight = new VertexPositionNormalTextureColor(textureTopRight, normal, uvTopRight, faceColor);
			var bottomLeft = new VertexPositionNormalTextureColor(textureBottomLeft, normal, uvBottomLeft,
				faceColor);
			var bottomRight = new VertexPositionNormalTextureColor(textureBottomRight, normal, uvBottomRight,
				faceColor);

			switch (blockFace)
			{
				case BlockFace.Up:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.Down:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
				case BlockFace.North:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
				case BlockFace.East:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.South:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.West:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
			}

			return new VertexPositionNormalTextureColor[0];
		}

		public override BoundingBox GetBoundingBox(V3 position, Block requestingBlock)
		{
			return new BoundingBox(position + Min, position + Max);
		}
	}

	public class CachedResourcePackModel : ResourcePackModel
	{
		public CachedResourcePackModel(ResourceManager resources, BlockStateModel variant) : base(resources, variant)
		{
			Cache();
		}

		private class FaceCache
		{
			public VertexPositionNormalTextureColor[] Up;
			public VertexPositionNormalTextureColor[] Down;
			public VertexPositionNormalTextureColor[] West;
			public VertexPositionNormalTextureColor[] East;
			public VertexPositionNormalTextureColor[] South;
			public VertexPositionNormalTextureColor[] North;
			public VertexPositionNormalTextureColor[] None;
		}

		private Dictionary<int, FaceCache> _elementCache = new Dictionary<int, FaceCache>();

		private void Cache()
		{
			var modelRotationMatrix =
				Matrix.CreateRotationX((float)MathUtils.ToRadians(360f - Variant.X)) *
				Matrix.CreateRotationY((float)MathUtils.ToRadians(360f - Variant.Y));

			foreach (var element in Variant.Model.Elements)
			{
				var c = new V3(8f, 8f, 8f);

				var elementFrom = new V3((element.From.X), (element.From.Y),
					(element.From.Z));

				var elementTo = new V3((element.To.X), (element.To.Y),
					(element.To.Z));

				var elementModelRotation = Matrix.CreateTranslation(-c) * modelRotationMatrix *
										   Matrix.CreateTranslation(c);

				FaceCache elementCache = new FaceCache();
				foreach (var face in element.Faces)
				{
					var faceStart = elementFrom;
					var faceEnd = elementTo;

					float x1 = 0, x2 = 1 / 32f, y1 = 0, y2 = 1 / 32f;
					if (Resources != null)
					{
						string textureName = "";
						if (!Variant.Model.Textures.TryGetValue(face.Value.Texture.Replace("#", ""), out textureName))
						{
							textureName = face.Value.Texture;
						}

						if (textureName.StartsWith("#"))
						{
							if (!Variant.Model.Textures.TryGetValue(textureName.Replace("#", ""), out textureName))
							{
								textureName = "no_texture";
							}
						}

						var textureInfo = Resources.Atlas.GetAtlasLocation(textureName.Replace("blocks/", ""));
						var textureLocation = textureInfo.Position;

						var uvSize = Resources.Atlas.AtlasSize;

						var pixelSizeX = (textureInfo.Width / uvSize.X) / 16f; //0.0625
						var pixelSizeY = (textureInfo.Height / uvSize.Y) / 16f;

						var uv = face.Value.UV;

						textureLocation.X /= uvSize.X;
						textureLocation.Y /= uvSize.Y;

						x1 = textureLocation.X + (uv.X1 * pixelSizeX);
						x2 = textureLocation.X + (uv.X2 * pixelSizeX);
						y1 = textureLocation.Y + (uv.Y1 * pixelSizeY);
						y2 = textureLocation.Y + (uv.Y2 * pixelSizeY);
					}

					var uvmap = new UVMap(new Microsoft.Xna.Framework.Vector2(x1, y1),
						new Microsoft.Xna.Framework.Vector2(x2, y1), new Microsoft.Xna.Framework.Vector2(x1, y2),
						new Microsoft.Xna.Framework.Vector2(x2, y2), Color.White, Color.White, Color.White);

					Matrix elementRotationMatrix = Matrix.Identity;
					float ci = 0f;
					var elementRotation = element.Rotation;
					if (elementRotation.Axis != Axis.Undefined)
					{
						var elementRotationOrigin = new V3(elementRotation.Origin.X, elementRotation.Origin.Y, elementRotation.Origin.Z);

						var elementAngle =
							MathUtils.ToRadians((float)(elementRotation.Axis == Axis.X ? -elementRotation.Angle : elementRotation.Angle));
						elementAngle = elementRotation.Axis == Axis.Z ? elementAngle : -elementAngle;
						ci = 1f / (float)Math.Cos(elementAngle);

						elementRotationMatrix = Matrix.CreateTranslation(-elementRotationOrigin);
						if (elementRotation.Axis == ResourcePackLib.Json.Axis.X)
						{
							elementRotationMatrix *= Matrix.CreateRotationX(elementAngle);
						}
						else if (elementRotation.Axis == ResourcePackLib.Json.Axis.Y)
						{
							elementRotationMatrix *= Matrix.CreateRotationY(elementAngle);
						}
						else if (elementRotation.Axis == ResourcePackLib.Json.Axis.Z)
						{
							elementRotationMatrix *= Matrix.CreateRotationZ(elementAngle);
						}

						elementRotationMatrix *= Matrix.CreateTranslation(elementRotationOrigin);
					}

					VertexPositionNormalTextureColor[] faceVertices = GetFaceVertices(face.Value, face.Key, faceStart, faceEnd, uvmap);
					for (var index = 0; index < faceVertices.Length; index++)
					{
						var vert = faceVertices[index];

						if (elementRotation.Axis != Axis.Undefined)
						{
							vert.Position = V3.Transform(vert.Position, elementRotationMatrix);

							if (elementRotation.Rescale)
							{
								if (elementRotation.Axis == Axis.X || elementRotation.Axis == Axis.Z)
								{
									vert.Position.Y *= ci;
								}

								if (elementRotation.Axis == Axis.Y || elementRotation.Axis == Axis.Z)
								{
									vert.Position.X *= ci;
								}

								if (elementRotation.Axis == Axis.Y || elementRotation.Axis == Axis.X)
								{
									vert.Position.Z *= ci;
								}
							}
						}

						vert.Position = V3.Transform(vert.Position, elementModelRotation);

						vert.Position = (vert.Position / 16f);

						faceVertices[index] = vert;
					}

					switch (face.Key)
					{
						case BlockFace.Down:
							elementCache.Down = faceVertices;
							break;
						case BlockFace.Up:
							elementCache.Up = faceVertices;
							break;
						case BlockFace.East:
							elementCache.East = faceVertices;
							break;
						case BlockFace.West:
							elementCache.West = faceVertices;
							break;
						case BlockFace.North:
							elementCache.North = faceVertices;
							break;
						case BlockFace.South:
							elementCache.South = faceVertices;
							break;
						case BlockFace.None:
							elementCache.None = faceVertices;
							break;
					}
				}

				_elementCache.Add(element.GetHashCode(), elementCache);
			}
		}

		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, V3 position, Block baseBlock)
		{
			var verts = new List<VertexPositionNormalTextureColor>();

			var modelRotationMatrix =
				Matrix.CreateRotationX((float)MathUtils.ToRadians(360f - Variant.X)) *
				Matrix.CreateRotationY((float)MathUtils.ToRadians(360f - Variant.Y));

			// MaxY = 0;
			V3 worldPosition = new V3(position.X, position.Y, position.Z);

			foreach (var element in Variant.Model.Elements)
			{
				var elementCache = _elementCache[element.GetHashCode()];
				foreach (var face in element.Faces)
				{
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

					cullFace = V3.Transform(cullFace, modelRotationMatrix);

					if (cullFace != V3.Zero && !CanRender(world, baseBlock, worldPosition + cullFace))
						continue;

					VertexPositionNormalTextureColor[] faceVertices;
					switch (face.Key)
					{
						case BlockFace.Down:
							faceVertices = (VertexPositionNormalTextureColor[]) elementCache.Down.Clone();
							break;
						case BlockFace.Up:
							faceVertices = (VertexPositionNormalTextureColor[])elementCache.Up.Clone();
							break;
						case BlockFace.East:
							faceVertices = (VertexPositionNormalTextureColor[])elementCache.East.Clone();
							break;
						case BlockFace.West:
							faceVertices = (VertexPositionNormalTextureColor[])elementCache.West.Clone();
							break;
						case BlockFace.North:
							faceVertices = (VertexPositionNormalTextureColor[])elementCache.North.Clone();
							break;
						case BlockFace.South:
							faceVertices = (VertexPositionNormalTextureColor[])elementCache.South.Clone();
							break;
						default:
							faceVertices = (VertexPositionNormalTextureColor[])elementCache.None.Clone();
							break;
					}

					Color faceColor = faceVertices[0].Color;
					if (element.Shade)
					{
						faceColor = UvMapHelp.AdjustColor(faceColor, cull, GetLight(world, worldPosition + cullFace));
					}

					for (var index = 0; index < faceVertices.Length; index++)
					{
						var vert = faceVertices[index];
						vert.Color = faceColor;

						vert.Position = worldPosition + vert.Position;

						verts.Add(vert);
					}
				}
			}

			return verts.ToArray();
		}
	}

	public static class VectorExtension {
		public static V3 From(V3 x, V3 y, V3 z)
		{
			return new V3(x.X, y.Y, z.Z);
		}
	}
}
