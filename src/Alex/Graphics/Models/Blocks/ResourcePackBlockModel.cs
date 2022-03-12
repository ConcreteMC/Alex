using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Common.Utils.Noise;
using Alex.Common.Utils.Vectors;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Alex.Worlds.Singleplayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using NLog;
using Matrix = System.Drawing.Drawing2D.Matrix;

namespace Alex.Graphics.Models.Blocks
{
	public class BlockStateModelWrapper
	{
		public BlockStateModel BlockStateModel { get; }
		public ResourcePackModelBase BlockModel { get; }

		public BlockStateModelWrapper(BlockStateModel model, ResourcePackModelBase blockModel)
		{
			BlockStateModel = model;
			BlockModel = blockModel;
		}
	}

	public class ResourcePackBlockModel : BlockModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));
		private static readonly SimplexPerlin NoiseGenerator = new SimplexPerlin(1337);

		static ResourcePackBlockModel() { }

		public static bool SmoothLighting { get; set; } = true;
		private ResourceManager Resources { get; }

		public ResourcePackBlockModel(ResourceManager resources)
		{
			Resources = resources;
		}

		/// <inheritdoc />
		public override IEnumerable<BoundingBox> GetBoundingBoxes(BlockState blockState, Vector3 blockPos)
		{
			List<BoundingBox> boxes = new List<BoundingBox>();

			foreach (var model in GetAppliedModels(blockState))
			{
				boxes.AddRange(GenerateBoundingBoxes(model.BlockStateModel, model.BlockModel));
			}

			//blockState.BoundingBoxes = boxes.ToArray();


			foreach (var box in boxes)
			{
				var x = box;

				if (x.Min == new Vector3(float.MaxValue) || x.Max == new Vector3(float.MinValue))
					continue;

				var dimensions = x.GetDimensions();

				if (dimensions.X < 0.015f)
				{
					var diff = (0.015f - dimensions.X) / 2f;
					x.Min.X -= diff;
					x.Max.X += diff;
					//box.Inflate(new Vector3(0.015f - dimensions.X, 0f, 0f));
				}

				if (dimensions.Y < 0.015f)
				{
					var diff = (0.015f - dimensions.Y) / 2f;
					x.Min.Y -= diff;
					x.Max.Y += diff;
					//box.Inflate(new Vector3(0f, 0.015f - dimensions.Y, 0f));
				}

				if (dimensions.Z < 0.015f)
				{
					var diff = (0.015f - dimensions.Z) / 2f;
					x.Min.Z -= diff;
					x.Max.Z += diff;
					//box.Inflate(new Vector3(0f, 0f, 0.015f - dimensions.Z));
				}

				yield return x.OffsetBy(blockPos);
				//yield return box;
			}
		}

		protected virtual bool ShouldRenderFace(IBlockAccess world, BlockFace face, BlockCoordinates position, Block me)
		{
			if (world == null) return true;

			if (position.Y >= 256) return true;

			if (face == BlockFace.None)
				return true;

			var pos = position + face.GetBlockCoordinates();

			var theBlock = world.GetBlockState(pos)?.Block;

			if (theBlock == null || !theBlock.Renderable)
				return true;

			return me.ShouldRenderFace(face, theBlock);
		}


		private IEnumerable<BoundingBox> GenerateBoundingBoxes(BlockStateModel stateModel,
			ResourcePackLib.Json.Models.ResourcePackModelBase model)
		{
			for (var index = 0; index < model.Elements.Length; index++)
			{
				var eMinX = float.MaxValue;
				var eMinY = float.MaxValue;
				var eMinZ = float.MaxValue;

				var eMaxX = float.MinValue;
				var eMaxY = float.MinValue;
				var eMaxZ = float.MinValue;

				var element = model.Elements[index];
				element.To *= Scale;
				element.From *= Scale;

				foreach (var face in element.Faces)
				{
					var facing = face.Key;

					if (stateModel.X > 0f)
					{
						var offset = stateModel.X / 90;
						facing = RotateDirection(facing, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
					}

					if (stateModel.Y > 0f)
					{
						var offset = stateModel.Y / 90;
						facing = RotateDirection(facing, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
					}

					var verts = GetFaceVertices(face.Key, element.From, element.To, new BlockTextureData());
					verts = ProcessVertices(verts, stateModel, element, null, facing, face.Value);

					float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
					float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

					for (int i = 0; i < verts.Length; i++)
					{
						var v = verts[i];

						if (v.Position.X < minX)
						{
							minX = v.Position.X;
						}
						else if (v.Position.X > maxX)
						{
							maxX = v.Position.X;
						}

						if (v.Position.Y < minY)
						{
							minY = v.Position.Y;
						}
						else if (v.Position.Y > maxY)
						{
							maxY = v.Position.Y;
						}

						if (v.Position.Z < minZ)
						{
							minZ = v.Position.Z;
						}
						else if (v.Position.Z > maxZ)
						{
							maxZ = v.Position.Z;
						}

						//
						verts[i] = v;
					}

					eMinX = Math.Min(eMinX, minX);
					eMaxX = Math.Max(eMaxX, maxX);

					eMinY = Math.Min(eMinY, minY);
					eMaxY = Math.Max(eMaxY, maxY);

					eMinZ = Math.Min(eMinZ, minZ);
					eMaxZ = Math.Max(eMaxZ, maxZ);
				}

				yield return new BoundingBox(new Vector3(eMinX, eMinY, eMinZ), new Vector3(eMaxX, eMaxY, eMaxZ));
			}
		}

		private Vector3 FixRotation(Vector3 v, ModelElement element, BlockStateModel bsModel)
		{
			if (element.Rotation.Axis != Axis.Undefined)
			{
				var r = element.Rotation;
				var angle = (float)(r.Angle * (Math.PI / 180f));
				angle = (element.Rotation.Axis == Axis.Z) ? angle : -angle;

				var ci = 1.0f / MathF.Cos(angle);

				var origin = r.Origin;

				var c = MathF.Cos(angle);
				var s = MathF.Sin(angle);

				v.X -= (origin.X);
				v.Y -= (origin.Y);
				v.Z -= (origin.Z);

				switch (r.Axis)
				{
					case Axis.Y:
					{
						var x = v.X;
						var z = v.Z;

						v.X = (x * c - z * s);
						v.Z = (z * c + x * s);

						if (r.Rescale)
						{
							v.X *= ci;
							v.Z *= ci;
						}
					}

						break;

					case Axis.X:
					{
						var x = v.Z;
						var z = v.Y;

						v.Z = (x * c - z * s);
						v.Y = (z * c + x * s);

						if (r.Rescale)
						{
							v.Z *= ci;
							v.Y *= ci;
						}
					}

						break;

					case Axis.Z:
					{
						var x = v.X;
						var z = v.Y;

						v.X = (x * c - z * s);
						v.Y = (z * c + x * s);

						if (r.Rescale)
						{
							v.X *= ci;
							v.Y *= ci;
						}
					}

						break;
				}

				v.X += (origin.X);
				v.Y += (origin.Y);
				v.Z += (origin.Z);
			}

			if (bsModel.X > 0)
			{
				var rotX = bsModel.X * (MathHelper.Pi / 180f);
				var cc = MathF.Cos(rotX);
				var ss = MathF.Sin(rotX);
				var z = v.Z - 8f;
				var y = v.Y - 8f;

				v.Z = 8f + (z * cc - y * ss);
				v.Y = 8f + (y * cc + z * ss);
			}

			if (bsModel.Y > 0)
			{
				var rotY = bsModel.Y * (MathHelper.Pi / 180f);
				var cc = MathF.Cos(rotY);
				var ss = MathF.Sin(rotY);
				var x = v.X - 8f;
				var z = v.Z - 8f;

				v.X = 8f + (x * cc - z * ss);
				v.Z = 8f + (z * cc + x * ss);
			}

			return v;
		}

		private BlockShaderVertex[] ProcessVertices(BlockShaderVertex[] vertices,
			BlockStateModel bsModel,
			ModelElement element,
			BlockTextureData? uvMap,
			BlockFace blockFace,
			ModelElementFace face)
		{
			for (int i = 0; i < vertices.Length; i++)
			{
				var v = vertices[i];


				v.Position = FixRotation(v.Position, element, bsModel);


				if (uvMap.HasValue)
				{
					var tw = uvMap.Value.TextureInfo.FrameWidth;
					var th = uvMap.Value.TextureInfo.FrameHeight;

					var rot = face.Rotation;

					if (rot > 0)
					{
						var rotY = rot * (MathHelper.Pi / 180f);
						var c = MathF.Cos(rotY);
						var s = MathF.Sin(rotY);

						var x = v.TexCoords.X - 0.5f * tw;
						var y = v.TexCoords.Y - 0.5f * th;

						v.TexCoords.X = 0.5f * tw + (x * c - y * s);
						v.TexCoords.Y = 0.5f * th + (y * c + x * s);
					}

					if (bsModel.Uvlock)
					{
						if (bsModel.Y > 0 && (blockFace == BlockFace.Up || blockFace == BlockFace.Down))
						{
							var rotY = bsModel.Y * (MathHelper.Pi / 180f);
							var c = MathF.Cos(rotY);
							var s = MathF.Sin(rotY);
							var x = v.TexCoords.X - 0.5f * tw;
							var y = v.TexCoords.Y - 0.5f * th;

							v.TexCoords.X = 0.5f * tw + (x * c - y * s);
							v.TexCoords.Y = 0.5f * th + (y * c + x * s);
						}

						if (bsModel.X > 0 && (blockFace != BlockFace.Up && blockFace != BlockFace.Down))
						{
							var rotX = bsModel.X * (MathHelper.Pi / 180f);
							var c = MathF.Cos(rotX);
							var s = MathF.Sin(rotX);
							var x = v.TexCoords.X - 0.5f * tw;
							var y = v.TexCoords.Y - 0.5f * th;

							v.TexCoords.X = 0.5f * tw + (x * c - y * s);
							v.TexCoords.Y = 0.5f * th + (y * c + x * s);
						}
					}
				}

				v.Position /= 16f;
				v.Face = blockFace;
				vertices[i] = v;
			}

			return vertices;
		}

		private void CalculateModel(IBlockAccess world,
			ChunkData chunkBuilder,
			BlockCoordinates position,
			Block baseBlock,
			BlockStateModel blockStateModel,
			ResourcePackModelBase model,
			Biome biome)
		{
			var baseColor = baseBlock.BlockMaterial.TintColor;

			for (var index = 0; index < model.Elements.Length; index++)
			{
				var element = model.Elements[index];
				element.To *= Scale;
				element.From *= Scale;

				foreach (var face in element.Faces)
				{
					var facing = face.Key;
					var cullFace = face.Value.CullFace ?? face.Key;

					if (blockStateModel.X > 0f)
					{
						var offset = blockStateModel.X / 90;
						cullFace = RotateDirection(cullFace, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
						facing = RotateDirection(facing, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
					}

					if (blockStateModel.Y > 0f)
					{
						var offset = blockStateModel.Y / 90;
						cullFace = RotateDirection(cullFace, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
						facing = RotateDirection(facing, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
					}

					float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
					float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

					if (!ShouldRenderFace(world, facing, position, baseBlock))
						continue;

					var uv = face.Value.Uv;
					float x1 = 0, x2 = 0, y1 = 0, y2 = 0;

					if (uv == null)
					{
						switch (face.Key)
						{
							case BlockFace.North:
							case BlockFace.South:
								x1 = element.From.X;
								x2 = element.To.X;
								y1 = 16f - element.To.Y;
								y2 = 16f - element.From.Y;

								break;

							case BlockFace.West:
							case BlockFace.East:
								x1 = element.From.Z;
								x2 = element.To.Z;
								y1 = 16f - element.To.Y;
								y2 = 16f - element.From.Y;

								break;

							case BlockFace.Down:
							case BlockFace.Up:
								x1 = element.From.X;
								x2 = element.To.X;
								y1 = 16f - element.To.Z;
								y2 = 16f - element.From.Z;

								break;
						}
					}
					else
					{
						x1 = uv.X1;
						y1 = uv.Y1;
						x2 = uv.X2;
						y2 = uv.Y2;
					}

					var faceColor = baseColor;

					bool hasTint = (face.Value.TintIndex.HasValue && face.Value.TintIndex == 0);

					if (hasTint)
					{
						switch (baseBlock.BlockMaterial.TintType)
						{
							case TintType.Default:
								faceColor = Color.White;

								break;

							case TintType.Color:
								faceColor = baseBlock.BlockMaterial.TintColor;

								break;

							case TintType.Grass:
								if (SmoothLighting)
								{
									var bx = (int)position.X;
									var y = (int)position.Y;
									var bz = (int)position.Z;

									faceColor = CombineColors(
										GetBiomeGrassColor(world, bx, y, bz), GetBiomeGrassColor(world, bx - 1, y, bz),
										GetBiomeGrassColor(world, bx, y, bz - 1),
										GetBiomeGrassColor(world, bx + 1, y, bz),
										GetBiomeGrassColor(world, bx, y, bz + 1),
										GetBiomeGrassColor(world, bx + 1, y, bz - 1));
								}
								else
								{
									if (biome.GrassColor != null)
									{
										faceColor = biome.GrassColor.Value;
									}
									else
									{
										faceColor = Resources.GetGrassColor(
											biome.Temperature, biome.Downfall, (int)position.Y);
									}
								}

								break;

							case TintType.Foliage:
								if (face.Value.TintIndex.HasValue && face.Value.TintIndex == 0)
								{
									if (biome.FoliageColor != null)
									{
										faceColor = biome.FoliageColor.Value;
									}
									else
									{
										faceColor = Resources.GetFoliageColor(
											biome.Temperature, biome.Downfall, (int)position.Y);
									}
								}

								break;

							default:
								throw new ArgumentOutOfRangeException();
						}
					}

					faceColor = AdjustColor(faceColor, facing, element.Shade);

					BlockTextureData uvMap = GetTextureUVMap(
						Resources, face.Value.Texture, x1, x2, y1, y2, face.Value.Rotation, faceColor, null);

					var vertices = GetFaceVertices(face.Key, element.From, element.To, uvMap);

					vertices = ProcessVertices(vertices, blockStateModel, element, uvMap, facing, face.Value);

					RenderStage targetState = RenderStage.Opaque;

					if (baseBlock.BlockMaterial.IsLiquid)
					{
						targetState = RenderStage.Animated;
					}
					else if (uvMap.IsAnimated)
					{
						targetState = RenderStage.Animated;
					}
					else if (baseBlock.Transparent)
					{
						if (baseBlock.BlockMaterial.IsOpaque)
						{
							if (!Block.FancyGraphics && baseBlock.IsFullCube)
							{
								targetState = RenderStage.Opaque;
							}
							else
							{
								targetState = RenderStage.Transparent;
							}
						}
						else
						{
							targetState = RenderStage.Translucent;
						}
					}
					else if (!baseBlock.IsFullCube)
					{
						targetState = RenderStage.Opaque;
					}

					var vertexFlags = VertexFlags.None;

					if (baseBlock.Solid)
						vertexFlags |= VertexFlags.Solid;

					if (baseBlock.Transparent)
						vertexFlags |= VertexFlags.Transparent;

					if (baseBlock.IsFullCube)
						vertexFlags |= VertexFlags.FullCube;

					for (int i = 0; i < vertices.Length; i++)
					{
						var vertex = vertices[i];
						var textureCoordinates = (vertex.TexCoords) + uvMap.TextureInfo.Position;

						chunkBuilder.AddVertex(
							position, vertex.Position, vertex.Face,
							new Vector4(
								textureCoordinates.X, textureCoordinates.Y, uvMap.TextureInfo.Width,
								uvMap.TextureInfo.Height), vertex.Color, targetState, vertexFlags);
					}
				}
			}
		}

		private Color GetBiomeFoliageColor(IBlockAccess access, int x, int y, int z)
		{
			var biome = access.GetBiome(new BlockCoordinates(x, y, z));

			return Resources.GetFoliageColor(biome.Temperature, biome.Downfall, y);
		}

		private Color GetBiomeGrassColor(IBlockAccess access, int x, int y, int z)
		{
			var biome = access.GetBiome(new BlockCoordinates(x, y, z));

			return Resources.GetGrassColor(biome.Temperature, biome.Downfall, y);
		}

		private IEnumerable<BlockStateModelWrapper> GetAppliedModels(BlockState baseBlock)
		{
			if (baseBlock.ModelData == null)
				yield break;

			if (baseBlock.VariantMapper.IsMultiPart)
			{
				foreach (var model in baseBlock.ModelData)
				{
					if (Resources.BlockModelRegistry.TryGet(model.ModelName, out var registryEntry))
					{
						yield return new BlockStateModelWrapper(model, registryEntry.Value);
					}
				}

				yield break;
			}

			//	if (UseRandomizer)
			{
				BlockStateModel selectedModel = null;

				if (baseBlock.ModelData.Count > 1)
				{
					var weightSum = baseBlock.ModelData.Sum(x => x.Weight);

					var rnd = weightSum;
					
					foreach (var model in baseBlock.ModelData)
					{
						//	var model = Models[index];
						rnd -= model.Weight;

						if (rnd < 0)
						{
							selectedModel = model;

							break;
						}
					}
				}
				else
				{
					selectedModel = baseBlock.ModelData.FirstOrDefault();
				}

				if (selectedModel == null)
					yield break;

				if (Resources.BlockModelRegistry.TryGet(selectedModel.ModelName, out var registryEntry))
				{
					yield return new BlockStateModelWrapper(selectedModel, registryEntry.Value);
				}
			}
		}

		public override void GetVertices(IBlockAccess world,
			ChunkData chunkBuilder,
			BlockCoordinates position,
			BlockState baseBlock)
		{
			var biome = world == null ? BiomeUtils.GetBiome(0) : world.GetBiome(position);


			if (baseBlock.ModelData == null)
				return;

			if (baseBlock.VariantMapper.IsMultiPart)
			{
				foreach (var model in baseBlock.ModelData)
				{
					if (Resources.BlockModelRegistry.TryGet(model.ModelName, out var registryEntry))
					{
						CalculateModel(
							world, chunkBuilder, position, baseBlock.Block, model, registryEntry.Value, biome);
					}
				}

				return;
			}

			//	if (UseRandomizer)
			{
				BlockStateModel selectedModel = null;

				if (baseBlock.ModelData.Count > 1)
				{
					var weightSum = baseBlock.ModelData.Sum(x => x.Weight);

					var rnd = ((baseBlock.ModelData.Count == 1) ? 1f : MathF.Abs(
						NoiseGenerator.GetValue(position.X * position.Y, position.Z * position.X))) * weightSum;

					foreach (var model in baseBlock.ModelData)
					{
						rnd -= model.Weight;

						if (rnd < 0)
						{
							selectedModel = model;

							break;
						}
					}
				}
				else
				{
					selectedModel = baseBlock.ModelData.FirstOrDefault();
				}

				if (selectedModel == null)
					return;

				if (Resources.BlockModelRegistry.TryGet(selectedModel.ModelName, out var registryEntry))
				{
					CalculateModel(
						world, chunkBuilder, position, baseBlock.Block, selectedModel, registryEntry.Value, biome);
				}
			}
		}
	}
}