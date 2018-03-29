using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.Utils;
using Alex.Worlds;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NLog;
using Axis = Alex.ResourcePackLib.Json.Axis;

namespace Alex.Graphics.Models.Blocks
{
	public class CachedResourcePackModel : ResourcePackModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));
		static CachedResourcePackModel()
		{
			
		}

		public CachedResourcePackModel(ResourceManager resources, BlockStateModel[] variant) : base(resources, variant)
		{
			if (variant != null)
			{
				_elementCache = CalculateModel(variant);
			}
		}

		protected class FaceCache
		{
			//public VertexPositionNormalTextureColor[] Up = new VertexPositionNormalTextureColor[0];
			//public VertexPositionNormalTextureColor[] Down = new VertexPositionNormalTextureColor[0];
			//public VertexPositionNormalTextureColor[] West = new VertexPositionNormalTextureColor[0];
			//public VertexPositionNormalTextureColor[] East = new VertexPositionNormalTextureColor[0];
			//public VertexPositionNormalTextureColor[] South = new VertexPositionNormalTextureColor[0];
			//public VertexPositionNormalTextureColor[] North = new VertexPositionNormalTextureColor[0];
			//public VertexPositionNormalTextureColor[] None = new VertexPositionNormalTextureColor[0];

			private Dictionary<BlockFace, VertexPositionNormalTextureColor[]> _cache = new Dictionary<BlockFace, VertexPositionNormalTextureColor[]>();
			public bool TryGet(BlockFace face, out VertexPositionNormalTextureColor[] vertices)
			{
				return _cache.TryGetValue(face, out vertices);
			}

			public void Set(BlockFace face, VertexPositionNormalTextureColor[] vertices)
			{
				_cache[face] = vertices;
			}
		}

		private IDictionary<int, FaceCache> _elementCache;

		protected IDictionary<int, FaceCache> CalculateModel(BlockStateModel[] models)
		{
			Dictionary<int, FaceCache> result = new Dictionary<int, FaceCache>();
			foreach (var model in models)
			{
				foreach (var element in model.Model.Elements)
				{
					var elementFrom = new Vector3((element.From.X), (element.From.Y),
						(element.From.Z));

					var elementTo = new Vector3((element.To.X), (element.To.Y),
						(element.To.Z));

					var elementRotation = element.Rotation;
					Matrix elementRotationMatrix = GetElementRotationMatrix(elementRotation, out float scalingFactor);

					FaceCache elementCache = new FaceCache();
					foreach (var face in element.Faces)
					{
						var uv = face.Value.UV;

						var text = ResolveTexture(model, face.Value.Texture);

						var uvmap = GetTextureUVMap(Resources, text, uv.X1, uv.X2, uv.Y1, uv.Y2, face.Value.Rotation);

						VertexPositionNormalTextureColor[] faceVertices = GetFaceVertices(face.Key, elementFrom, elementTo, uvmap);

						float minX = 1f, minY = 1f, minZ = 1f;
						float maxX = -1f, maxY = -1f, maxZ = -1f;

						for (var index = 0; index < faceVertices.Length; index++)
						{
							var vert = faceVertices[index];

							//Apply element rotation
							if (elementRotation.Axis != Axis.Undefined)
							{
								vert.Position = Vector3.Transform(vert.Position, elementRotationMatrix);

								//Scale the texture back to its correct size
								if (elementRotation.Rescale) 
								{
									if (elementRotation.Axis == Axis.X || elementRotation.Axis == Axis.Z)
									{
										vert.Position.Y *= scalingFactor;
									}

									if (elementRotation.Axis == Axis.Y || elementRotation.Axis == Axis.Z)
									{
										vert.Position.X *= scalingFactor;
									}

									if (elementRotation.Axis == Axis.Y || elementRotation.Axis == Axis.X)
									{
										vert.Position.Z *= scalingFactor;
									}
								}
							}

							/*if (var.X > 0.0f)
							{
								var rotx = MathUtils.ToRadians((var.X + 360f) % 360f);//(var.X * (MathF.PI / 180.0f));
								var cos = MathF.Cos(rotx);
								var sin = MathF.Sin(rotx);
								var z = vert.Position.Z - 0.5f;
								var y = vert.Position.Y - 0.5f;
								vert.Position.Z = 0.5f + (z * cos - y * sin);
								vert.Position.Y = 0.5f + (y * cos + z * sin);
							}

							if (var.Y > 0.0f) {
								var roty = MathUtils.ToRadians((var.Y + 360f) % 360f);//(var.Y * (MathF.PI / 180.0f));
								var cos = MathF.Cos(roty);
								var sin = MathF.Sin(roty);
								var x = vert.Position.X - 0.5f;
								var z = vert.Position.Z - 0.5f;
								vert.Position.X = 0.5f + (x * cos - z * sin);
								vert.Position.Z = 0.5f + (z * cos + x * sin);
							}*/

							vert.Position = Vector3.Transform(vert.Position, Matrix.CreateTranslation(-element.Rotation.Origin) * GetModelRotationMatrix(model) * Matrix.CreateTranslation(element.Rotation.Origin));

							//Scale the position
							vert.Position = (vert.Position / 16f);

							if (vert.Position.X < minX)
							{
								minX = vert.Position.X;
							}
							else if (vert.Position.X > maxX)
							{
								maxX = vert.Position.X;
							}
							if (vert.Position.Y < minY)
							{
								minY = vert.Position.Y;
							}
							else if (vert.Position.Y > maxY)
							{
								maxY = vert.Position.Y;
							}
							if (vert.Position.Z < minZ)
							{
								minZ = vert.Position.Z;
							}
							else if (vert.Position.Z > maxZ)
							{
								maxZ = vert.Position.Z;
							}

							faceVertices[index] = vert;
						}

						base.Min = Vector3.Min(new Vector3(minX, minY, minZ), Min);
						base.Max = Vector3.Max(new Vector3(maxX, maxY, maxZ), Max);

						elementCache.Set(face.Key, faceVertices);
					}

					result.Add(element.GetHashCode(), elementCache);
				}
			}

			return result;
		}

		protected VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, IBlock baseBlock,
			BlockStateModel[] models, IDictionary<int, FaceCache> faceCache)
		{
			var verts = new List<VertexPositionNormalTextureColor>(36);

			// MaxY = 0;
			Vector3 worldPosition = new Vector3(position.X, position.Y, position.Z);

			foreach (var model in models)
			{
				foreach (var element in model.Model.Elements)
				{
					FaceCache elementCache;
					if (!faceCache.TryGetValue(element.GetHashCode(), out elementCache))
					{
						Log.Warn($"Element cache is null!");
						continue;
					}

					foreach (var face in element.Faces)
					{
						GetCullFaceValues(face.Value.CullFace, face.Key, out var cull, out var cullFace);

						var facing = face.Key;

						if (model.X > 0f)
						{
							var o = model.X / 90;
							cull = RotateDirection(cull, o, FACE_ROTATION_X, new BlockFace[]
							{
								BlockFace.East,
								BlockFace.West,
								BlockFace.None
							});

							facing = RotateDirection(facing, o, FACE_ROTATION_X, new BlockFace[]
							{
								BlockFace.East,
								BlockFace.West,
								BlockFace.None
							});
						}

						if (model.Y > 0f)
						{
							var o = model.Y / 90;
							cull = RotateDirection(cull, o, FACE_ROTATION, new BlockFace[]
							{
								BlockFace.Up,
								BlockFace.Down,
								BlockFace.None
							});

							facing = RotateDirection(facing, o, FACE_ROTATION, new BlockFace[]
							{
								BlockFace.Up,
								BlockFace.Down,
								BlockFace.None
							});
						}

						//cullFace = Vector3.Transform(cullFace, modelRotationMatrix);

						if (cullFace != Vector3.Zero && !ShouldRenderFace(world, cull, worldPosition, baseBlock)/* CanRender(world, baseBlock, worldPosition, cull)*/)
							continue;

						VertexPositionNormalTextureColor[] faceVertices;
						if (!elementCache.TryGet(face.Key, out faceVertices) || faceVertices.Length == 0)
						{
							Log.Warn($"No vertices cached for face {face.Key} in model {model.ModelName}");
							continue;
						}

						Color faceColor = faceVertices[0].Color;

						if (face.Value.TintIndex >= 0)
						{
							int biomeId = world.GetBiome((int)worldPosition.X, 0, (int)worldPosition.Z);

							if (biomeId != -1)
							{
								var biome = BiomeUtils.GetBiomeById(biomeId);

								if (baseBlock.Name.Equals("minecraft:grass_block", StringComparison.InvariantCultureIgnoreCase))
								{
									faceColor = Resources.ResourcePack.GetGrassColor(biome.Temperature, biome.Downfall, (int)worldPosition.Y);
								}
								else
								{
									faceColor = Resources.ResourcePack.GetFoliageColor(biome.Temperature, biome.Downfall, (int)worldPosition.Y);
								}
							}
						}

						faceColor = LightingUtils.AdjustColor(faceColor, cull, GetLight(world, worldPosition + cullFace, model.Model.AmbientOcclusion), element.Shade);

						for (var index = 0; index < faceVertices.Length; index++)
						{
							var vert = faceVertices[index];
							vert.Color = faceColor;

							vert.Position = worldPosition + vert.Position;

							verts.Add(vert);
						}
					}
				}
			}

			return verts.ToArray();
		}

		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, IBlock baseBlock)
		{
			return GetVertices(world, position, baseBlock, Variant, _elementCache);
		}
	}
}