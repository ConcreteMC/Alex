using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.API.Utils.Noise;
using Alex.API.World;
using Alex.Blocks.Minecraft;
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
using NLog;
using MathF = Alex.API.Utils.MathF;
using Matrix = System.Drawing.Drawing2D.Matrix;

namespace Alex.Graphics.Models.Blocks
{
	public class ResourcePackBlockModel : BlockModel
	{
		private static readonly Logger        Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));
		private static          SimplexPerlin NoiseGenerator { get; } = new SimplexPerlin(1337);

		static ResourcePackBlockModel()
		{
			
		}
		
		public static           bool       SmoothLighting { get; set; } = true;
		
		private BlockStateModel[] Models    { get; set; }
		private ResourceManager   Resources { get; }

		private Vector3 _min = new Vector3(float.MaxValue);
		private Vector3 _max = new Vector3(float.MinValue);

		private  BoundingBox[] Boxes         { get; set; }
		private bool          UseRandomizer { get; set; }
		private int           WeightSum     { get; }
		public ResourcePackBlockModel(ResourceManager resources, BlockStateModel[] models, bool useRandomizer = false)
		{
			Resources = resources;

			Models = models;
			
			//Models = models;
			UseRandomizer = useRandomizer;
			WeightSum = models.Sum(x => x.Weight);
			
			Boxes = CalculateBoundingBoxes(Models);

			/*for (int i = 0; i < Boxes.Length; i++)
			{
				var box         = Boxes[i];
				
				/*var yDifference = box.Max.Y - box.Min.Y;
				if (yDifference < 0.01f)
				{
					box.Max.Y += (0.01f - yDifference);
				}

				var xDifference = box.Max.X - box.Min.X;
				if (xDifference < 0.01f)
				{
					box.Max.X += (0.01f - xDifference);
				}
				
				var zDifference = box.Max.Z - box.Min.Z;
				if (zDifference < 0.01f)
				{
					box.Max.Z += (0.01f - zDifference);
				}*
				
				Boxes[i] = box;
			}*/
		}

		/// <inheritdoc />
		public override IEnumerable<BoundingBox> GetBoundingBoxes(BlockCoordinates blockPos)
		{
			return Boxes.Select(x => x.OffsetBy(blockPos));
		}

		protected virtual bool ShouldRenderFace(IBlockAccess world, BlockFace face, BlockCoordinates position, Block me)
		{
			if (world == null) return true;
			
			if (position.Y >= 256) return true;

			if (face == BlockFace.None)
				return true;
				
			var pos = position + face.GetBlockCoordinates();

			var cX = (int)pos.X & 0xf;
			var cZ = (int)pos.Z & 0xf;

			if (cX < 0 || cX > 16)
				return false;

			if (cZ < 0 || cZ > 16)
				return false;
			
			var theBlock = world.GetBlockState(pos).Block;

			if (!theBlock.Renderable)
				return true;
			
			return me.ShouldRenderFace(face, theBlock);
		}
		
		protected BoundingBox[] CalculateBoundingBoxes(BlockStateModel[] models)
		{
			List<BoundingBox> boundingBoxes = new List<BoundingBox>();
			for (var index = 0; index < models.Length; index++)
			{
				var model = models[index];

				if (Resources.BlockModelRegistry.TryGet(model.ModelName, out var registryEntry))
				{
					var boxes = GenerateBoundingBoxes(model, registryEntry.Value, out Vector3 min, out Vector3 max);
					boundingBoxes.AddRange(boxes);
					
					if (max.X > _max.X)
						_max.X = max.X;

					if (max.Y > _max.Y)
						_max.Y = max.Y;

					if (max.Z > _max.Z)
						_max.Z = max.Z;

					if (min.X < _min.X)
						_min.X = min.X;

					if (min.Y < _min.Y)
						_min.Y = min.Y;

					if (min.Z < _min.Z)
						_min.Z = min.Z;
				}
			}

			return boundingBoxes.ToArray();
		}

		
		private BoundingBox[] GenerateBoundingBoxes(BlockStateModel stateModel, ResourcePackLib.Json.Models.ResourcePackModelBase model, out Vector3 min, out Vector3 max)
		{
			float facesMinX = float.MaxValue, facesMinY = float.MaxValue, facesMinZ = float.MaxValue;
			float facesMaxX = float.MinValue, facesMaxY = float.MinValue, facesMaxZ = float.MinValue;

			List<BoundingBox> boxes = new List<BoundingBox>();
			for (var index = 0; index < model.Elements.Length; index++)
			{
				var eMinX   = float.MaxValue;
				var eMinY   = float.MaxValue;
				var eMinZ   = float.MaxValue;
				
				var eMaxX = float.MinValue;
				var eMaxY = float.MinValue;
				var eMaxZ = float.MinValue;
				
				var element = model.Elements[index];
				element.To *= Scale;
				element.From *= Scale;

				foreach (var face in element.Faces)
				{
					var facing   = face.Key;

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
						
						if (v.Position.X < eMinX)
						{
							eMinX = v.Position.X;
						}
						else if (v.Position.X > eMaxX)
						{
							eMaxX = v.Position.X;
						}

						if (v.Position.Y < eMinY)
						{
							eMinY = v.Position.Y;
						}
						else if (v.Position.Y > eMaxY)
						{
							eMaxY = v.Position.Y;
						}

						if (v.Position.Z < eMinZ)
						{
							eMinZ = v.Position.Z;
						}
						else if (v.Position.Z > eMaxZ)
						{
							eMaxZ = v.Position.Z;
						}

						verts[i] = v;
					}

					if (minX < facesMinX)
					{
						facesMinX = minX;
					}
					else if (maxX >facesMaxX)
					{
						facesMaxX = maxX;
					}

					if (minY < facesMinY)
					{
						facesMinY = minY;
					}
					else if (maxY > facesMaxY)
					{
						facesMaxY = maxY;
					}

					if (minZ < facesMinZ)
					{
						facesMinZ = minZ;
					}
					else if (maxZ > facesMaxZ)
					{
						facesMaxZ = maxZ;
					}
				}
				
				boxes.Add(new BoundingBox(new Vector3(eMinX, eMinY, eMinZ), new Vector3(eMaxX, eMaxY, eMaxZ)));
			}

			min = new Vector3(facesMinX, facesMinY, facesMinZ);
			max = new Vector3(facesMaxX, facesMaxY, facesMaxZ);

			return boxes.ToArray();
		}

		private Vector3 FixRotation(
			Vector3 v,
			ModelElement element)
		{
			if (element.Rotation.Axis != Axis.Undefined)
			{
				var r      = element.Rotation;
				var angle  = (float) (r.Angle * (Math.PI / 180f));
				angle = (element.Rotation.Axis == Axis.Z) ? angle : -angle;
					
				var ci     = 1.0f / MathF.Cos(angle);
				
				var origin = r.Origin;
							
				var c = MathF.Cos(angle);
				var s = MathF.Sin(angle);

				v.X -= (origin.X / 16.0f);
				v.Y -= (origin.Y / 16.0f);
				v.Z -= (origin.Z / 16.0f);
				
				switch (r.Axis)
				{
					case Axis.Y:
					{
						var x = v.X;
						var z = v.Z;

						v.X = (x * c - z * s);
						v.Z = (z * c + x * s);
						
						if (r.Rescale) {
							v.X *= ci;
							v.Z *= ci;
						}
					}
						break;

					case Axis.X:
					{
						var x = v.Z ;
						var z = v.Y;

						v.Z = (x * c - z * s);
						v.Y =  (z * c + x * s);

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
				
				v.X += (origin.X / 16.0f);
				v.Y += (origin.Y / 16.0f);
				v.Z += (origin.Z / 16.0f);
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
				
				v.Position /= 16f;
				v.Position = FixRotation(v.Position, element);

				if (bsModel.X > 0)
				{
					var rotX = bsModel.X * (MathHelper.Pi / 180f);
					var c    = MathF.Cos(rotX);
					var s    = MathF.Sin(rotX);
					var z    = v.Position.Z - 0.5f;
					var y    = v.Position.Y - 0.5f;

					v.Position.Z = 0.5f + (z * c - y * s);
					v.Position.Y = 0.5f + (y * c + z * s);
				}

				if (bsModel.Y > 0)
				{
					var rotY = bsModel.Y * (MathHelper.Pi / 180f);
					var c    = MathF.Cos(rotY);
					var s    = MathF.Sin(rotY);
					var x    = v.Position.X - 0.5f;
					var z    = v.Position.Z - 0.5f;

					v.Position.X = 0.5f + (x * c - z * s);
					v.Position.Z = 0.5f + (z * c + x * s);
				}

				if (uvMap.HasValue)
				{
					var tw = uvMap.Value.TextureInfo.Width;
					var th = uvMap.Value.TextureInfo.Height;

					var rot = face.Rotation;

					if (rot > 0)
					{
						var rotY = rot * (MathHelper.Pi / 180f);
						var c    = MathF.Cos(rotY);
						var s    = MathF.Sin(rotY);
						var x    = v.TexCoords.X - 8f * tw;
						var y    = v.TexCoords.Y - 8f * th;

						v.TexCoords.X = 8f * tw + (x * c - y * s);
						v.TexCoords.Y = 8f * th + (y * c + x * s);
					}
					
					if (bsModel.Uvlock)
					{
						if (bsModel.Y > 0 && (blockFace == BlockFace.Up || blockFace == BlockFace.Down))
						{
							var rotY = bsModel.Y * (MathHelper.Pi / 180f);
							var c    = MathF.Cos(rotY);
							var s    = MathF.Sin(rotY);
							var x    = v.TexCoords.X - 8f * tw;
							var y    = v.TexCoords.Y - 8f * th;

							v.TexCoords.X = 8f * tw + (x * c - y * s);
							v.TexCoords.Y = 8f * th + (y * c + x * s);
						}

						if (bsModel.X > 0 && (blockFace != BlockFace.Up && blockFace != BlockFace.Down))
						{
							var rotX = bsModel.X * (MathHelper.Pi / 180f);
							var c    = MathF.Cos(rotX);
							var s    = MathF.Sin(rotX);
							var x    = v.TexCoords.X - 8f * tw;
							var y    = v.TexCoords.Y - 8f * th;

							v.TexCoords.X = 8f * tw + (x * c - y * s);
							v.TexCoords.Y = 8f * th + (y * c + x * s);
						}
					}
					
					v.TexCoords += uvMap.Value.TextureInfo.Position;
					v.TexCoords *= (Vector2.One / uvMap.Value.TextureInfo.AtlasSize);
				}

				v.Face = blockFace;
				vertices[i] = v;
			}

			return vertices;
		}

		private void CalculateModel(IBlockAccess world,
			BlockCoordinates blockCoordinates,
			ChunkData chunkBuilder,
			Vector3 position,
			Block baseBlock,
			BlockStateModel blockStateModel,
			Biome biome)
		{
			//bsModel.Y = Math.Abs(180 - bsModel.Y);

			if (Resources.BlockModelRegistry.TryGet(blockStateModel.ModelName, out var registryEntry))
			{
				var model = registryEntry.Value;

				var baseColor = baseBlock.BlockMaterial.TintColor;

				for (var index = 0; index < model.Elements.Length; index++)
				{
					var element = model.Elements[index];
					element.To *= Scale;

					element.From *= Scale;

					foreach (var face in element.Faces)
					{
						var facing   = face.Key;
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

						if (!ShouldRenderFace(world, facing, position, baseBlock))
							continue;

						var positionOffset = baseBlock.GetOffset(NoiseGenerator, position);

						var   uv = face.Value.UV;
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
							x2 = uv.X2;
							y1 = uv.Y1;
							y2 = uv.Y2;
						}

						var faceColor = baseColor;

						bool hasTint = face.Value.TintIndex.HasValue && face.Value.TintIndex == 0;

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
										var bx = (int) position.X;
										var y  = (int) position.Y;
										var bz = (int) position.Z;

										faceColor = CombineColors(
											GetGrassBiomeColor(world, bx, y, bz),
											GetGrassBiomeColor(world, bx - 1, y, bz),
											GetGrassBiomeColor(world, bx, y, bz - 1),
											GetGrassBiomeColor(world, bx + 1, y, bz),
											GetGrassBiomeColor(world, bx, y, bz + 1),
											GetGrassBiomeColor(world, bx + 1, y, bz - 1));
									}
									else
									{

										faceColor = Resources.ResourcePack.GetGrassColor(
											biome.Temperature, biome.Downfall, (int) position.Y);
									}

									break;

								case TintType.Foliage:
									faceColor = Resources.ResourcePack.GetFoliageColor(
										biome.Temperature, biome.Downfall, (int) position.Y);

									break;

								default:
									throw new ArgumentOutOfRangeException();
							}
						}

						faceColor = AdjustColor(faceColor, facing, element.Shade);

						var uvMap = GetTextureUVMap(
							Resources, ResolveTexture(model, face.Value.Texture), x1, x2, y1, y2, face.Value.Rotation, faceColor);

						var vertices = GetFaceVertices(face.Key, element.From, element.To, uvMap);

						vertices = ProcessVertices(vertices, blockStateModel, element, uvMap, facing, face.Value);

						RenderStage targetState = RenderStage.OpaqueFullCube;

						if (baseBlock.BlockMaterial.IsLiquid)
						{
							targetState = RenderStage.Liquid;
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
									targetState = RenderStage.OpaqueFullCube;
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

						for (int i = 0; i < vertices.Length; i++)
						{
							var vertex = vertices[i];

							BlockModel.GetLight(
								world, vertex.Position + position + vertex.Face.GetVector3(), out var blockLight,
								out var skyLight, true);

							chunkBuilder.AddVertex(
								blockCoordinates, vertex.Position + position + positionOffset, vertex.TexCoords,
								vertex.Color, blockLight, skyLight, targetState);
						}
					}
				}
			}
		}

		private Color GetGrassBiomeColor(IBlockAccess access, int x, int y, int z)
		{
			var biome = access.GetBiome(new BlockCoordinates(x, y, z));
			return Resources.ResourcePack.GetGrassColor(
				biome.Temperature, biome.Downfall, y);
		}

		public override void GetVertices(IBlockAccess world, ChunkData chunkBuilder, BlockCoordinates blockCoordinates, Vector3 position, Block baseBlock)
		{
			var biome = world == null ? BiomeUtils.GetBiomeById(0) : world.GetBiome(position);

			if (UseRandomizer)
			{
				BlockStateModel selectedModel = null;

				var rnd = MathF.Abs(NoiseGenerator.GetValue(position.X * position.Y, position.Z * position.X))
				          * WeightSum;

				for (var index = 0; index < Models.Length; index++)
				{
					var model = Models[index];
					rnd -= model.Weight;

					if (rnd < 0)
					{
						selectedModel = model;

						break;
					}
				}

				CalculateModel(world, blockCoordinates, chunkBuilder, position, baseBlock, selectedModel, biome);
			}
			else
			{
				for (var bsModelIndex = 0; bsModelIndex < Models.Length; bsModelIndex++)
				{
					var bsModel = Models[bsModelIndex];

					if (string.IsNullOrWhiteSpace(bsModel.ModelName)) continue;

					CalculateModel(world, blockCoordinates, chunkBuilder, position, baseBlock, bsModel, biome);
				}
			}
		}
	}
}