using System;
using System.Collections.Generic;
using Alex.CoreRT.API.Graphics;
using Alex.CoreRT.API.World;
using Alex.CoreRT.Blocks;
using Alex.CoreRT.Utils;
using Alex.CoreRT.Worlds;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using MiNET.Worlds;
using ResourcePackLib.CoreRT.Json;
using ResourcePackLib.CoreRT.Json.BlockStates;
using Axis = ResourcePackLib.CoreRT.Json.Axis;

namespace Alex.CoreRT.Graphics.Models
{
	public class CachedResourcePackModel : ResourcePackModel
	{
		static CachedResourcePackModel()
		{
			
		}

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
				var c = new Vector3(8f, 8f, 8f);

				var elementFrom = new Vector3((element.From.X), (element.From.Y),
					(element.From.Z));

				var elementTo = new Vector3((element.To.X), (element.To.Y),
					(element.To.Z));

				var width = elementTo.X - elementFrom.X;
				var height = elementTo.Y - elementFrom.Y;
				var length = elementTo.Z - elementFrom.Z;

				var origin = new Vector3((elementTo.X + elementFrom.X) / 2 - 8,
					(elementTo.X + elementFrom.X) / 2 - 8,
					(elementTo.X + elementFrom.X) / 2 - 8);

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

					Matrix faceRotationMatrix = Matrix.Identity;
					float ci = 0f;
					var elementRotation = element.Rotation;
					if (elementRotation.Axis != Axis.Undefined)
					{
						var elementRotationOrigin = new Vector3(elementRotation.Origin.X, elementRotation.Origin.Y, elementRotation.Origin.Z);

						var elementAngle =
							MathUtils.ToRadians((float)(elementRotation.Axis == Axis.X ? -elementRotation.Angle : elementRotation.Angle));
						elementAngle = elementRotation.Axis == Axis.Z ? elementAngle : -elementAngle;
						ci = 1f / (float)Math.Cos(elementAngle);

						faceRotationMatrix = Matrix.CreateTranslation(-elementRotationOrigin);
						if (elementRotation.Axis == Axis.X)
						{
							faceRotationMatrix *= Matrix.CreateRotationX(elementAngle);
						}
						else if (elementRotation.Axis == Axis.Y)
						{
							faceRotationMatrix *= Matrix.CreateRotationY(elementAngle);
						}
						else if (elementRotation.Axis == Axis.Z)
						{
							faceRotationMatrix *= Matrix.CreateRotationZ(elementAngle);
						}

						faceRotationMatrix *= Matrix.CreateTranslation(elementRotationOrigin);
					}

					VertexPositionNormalTextureColor[] faceVertices = GetFaceVertices(face.Key, faceStart, faceEnd, uvmap, face.Value.Rotation);
					for (var index = 0; index < faceVertices.Length; index++)
					{
						var vert = faceVertices[index];

						if (elementRotation.Axis != Axis.Undefined)
						{
							//Apply face rotation
							vert.Position = Vector3.Transform(vert.Position, faceRotationMatrix);

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

						//Apply element rotation
						vert.Position = Vector3.Transform(vert.Position, elementModelRotation);

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

		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, Block baseBlock)
		{
			var verts = new List<VertexPositionNormalTextureColor>();

			var modelRotationMatrix =
				Matrix.CreateRotationX((float)MathUtils.ToRadians(360f - Variant.X)) *
				Matrix.CreateRotationY((float)MathUtils.ToRadians(360f - Variant.Y));

			// MaxY = 0;
			Vector3 worldPosition = new Vector3(position.X, position.Y, position.Z);

			foreach (var element in Variant.Model.Elements)
			{
				var elementCache = _elementCache[element.GetHashCode()];
				foreach (var face in element.Faces)
				{
					Vector3 cullFace = Vector3.Zero;

					BlockFace cull;
					if (!Enum.TryParse(face.Value.CullFace, out cull))
					{
						cull = face.Key;
					}
					switch (cull)
					{
						case BlockFace.Up:
							cullFace = Vector3.Up;
							break;
						case BlockFace.Down:
							cullFace = Vector3.Down;
							break;
						case BlockFace.North:
							cullFace = Vector3.Backward;
							break;
						case BlockFace.South:
							cullFace = Vector3.Forward;
							break;
						case BlockFace.West:
							cullFace = Vector3.Left;
							break;
						case BlockFace.East:
							cullFace = Vector3.Right;
							break;
					}

					cullFace = Vector3.Transform(cullFace, modelRotationMatrix);

					if (cullFace != Vector3.Zero && !CanRender(world, baseBlock, worldPosition + cullFace))
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

					if (face.Value.TintIndex >= 0)
					{
						World w = (World)world;

						if (w.ChunkManager.TryGetChunk(
							new ChunkCoordinates(new PlayerLocation(worldPosition.X, 0, worldPosition.Z)),
							out IChunkColumn column))
						{
							Worlds.ChunkColumn realColumn = (Worlds.ChunkColumn)column;
							var biome = BiomeUtils.GetBiomeById(realColumn.GetBiome((int)worldPosition.X & 0xf, (int)worldPosition.Z & 0xf));

							if (baseBlock.BlockId == 2)
							{
								faceColor = Resources.ResourcePack.GetGrassColor(biome.Temperature, biome.Downfall, (int)worldPosition.Y);
							}
							else
							{
								faceColor = Resources.ResourcePack.GetFoliageColor(biome.Temperature, biome.Downfall, (int)worldPosition.Y);
							}
						}
					}

					//if (element.Shade)
					{
						faceColor = UvMapHelp.AdjustColor(faceColor, cull, GetLight(world, worldPosition + cullFace), element.Shade);
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
}