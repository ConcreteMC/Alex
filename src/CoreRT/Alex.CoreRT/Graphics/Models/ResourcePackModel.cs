using System;
using System.Collections.Generic;
using Alex.CoreRT.API.Graphics;
using Alex.CoreRT.API.World;
using Alex.CoreRT.Blocks;
using Alex.CoreRT.Utils;
using log4net;
using Microsoft.Xna.Framework;
using ResourcePackLib.CoreRT.Json;
using ResourcePackLib.CoreRT.Json.BlockStates;
using ResourcePackLib.CoreRT.Json.Models;
using Axis = ResourcePackLib.CoreRT.Json.Axis;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using V3 = Microsoft.Xna.Framework.Vector3;

namespace Alex.CoreRT.Graphics.Models
{
    public class ResourcePackModel : BlockModel
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
		        Matrix.CreateRotationX((float) MathUtils.ToRadians(180f - Variant.X)) *
		        Matrix.CreateRotationY((float) MathUtils.ToRadians(180f - Variant.Y));

			// MaxY = 0;
	        V3 worldPosition = new V3(position.X, position.Y, position.Z);

			foreach (var element in Variant.Model.Elements)
            {
	            var c = new V3(8f, 8f, 8f);

				var elementFrom = new V3((element.From.X), (element.From.Y),
		           (element.From.Z));

	            var elementTo = new V3((element.To.X), (element.To.Y) ,
		            (element.To.Z));

	            var width = elementTo.X - elementFrom.X;
	            var height = elementTo.Y - elementFrom.Y;
	            var length = elementTo.Z - elementFrom.Z;

	            var origin = new V3((elementTo.X + elementFrom.X) / 2 - 8,
		            (elementTo.X + elementFrom.X) / 2 - 8,
		            (elementTo.X + elementFrom.X) / 2 - 8);

				var elementModelRotation = Matrix.CreateTranslation(-c) * modelRotationMatrix *
	                            Matrix.CreateTranslation(c);

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

					var uvmap = new UVMap(
						new Microsoft.Xna.Framework.Vector2(x1, y1),
						new Microsoft.Xna.Framework.Vector2(x2, y1), 
						new Microsoft.Xna.Framework.Vector2(x1, y2),
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
						if (elementRotation.Axis == Axis.X)
						{
							elementRotationMatrix *= Matrix.CreateRotationX(elementAngle);
						}
						else if (elementRotation.Axis == Axis.Y)
						{
							elementRotationMatrix *= Matrix.CreateRotationY(elementAngle);
						}
						else if (elementRotation.Axis == Axis.Z)
						{
							elementRotationMatrix *= Matrix.CreateRotationZ(elementAngle);
						}

						elementRotationMatrix *= Matrix.CreateTranslation(elementRotationOrigin);
					}

					VertexPositionNormalTextureColor[] faceVertices = GetFaceVertices(face.Key, faceStart, faceEnd, uvmap, face.Value.Rotation);

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

		public override BoundingBox GetBoundingBox(V3 position, Block requestingBlock)
		{
			return new BoundingBox(position + Min, position + Max);
		}
	}

	public static class VectorExtension {
		public static V3 From(V3 x, V3 y, V3 z)
		{
			return new V3(x.X, y.Y, z.Z);
		}
	}
}
