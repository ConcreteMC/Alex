using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.Utils;
using Alex.Worlds;
using Collections.Pooled;
using Microsoft.Xna.Framework;
using NLog;
using MathF = Alex.API.Utils.MathF;
using Matrix = System.Drawing.Drawing2D.Matrix;

namespace Alex.Graphics.Models.Blocks
{
	public class ResourcePackBlockModel : BlockModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));
		private static FastRandom FastRandom { get; } = new FastRandom();

		public static bool SmoothLighting { get; set; } = true;
		
		private BlockStateModel[] Models { get; set; }
		protected ResourceManager Resources { get; }

		protected Vector3 Min = new Vector3(float.MaxValue);
		protected Vector3 Max = new Vector3(float.MinValue);

		public override BoundingBox BoundingBox
		{
			get
			{
				return new BoundingBox(Min, Max);
			}
		}

		public BoundingBox[] Boxes { get; set; } = new BoundingBox[0];
		private bool UseRandomizer { get; set; }
		public ResourcePackBlockModel(ResourceManager resources, BlockStateModel[] models, bool useRandomizer = false)
		{
			Resources = resources;
			Models = models.Select(x =>
			{
				return new BlockStateModel()
				{
					Uvlock = x.Uvlock,
					Weight = x.Weight,
					X = x.X,
					Y = x.Y,
					ModelName = x.ModelName,
					Model = new ResourcePackLib.Json.Models.Blocks.BlockModel()
					{
						AmbientOcclusion = x.Model.AmbientOcclusion,
						Display = null,
						Elements = x.Model.Elements.Select(el =>
						{
							return new BlockModelElement()
							{
								From = el.From,
								To = el.To,
								Rotation = el.Rotation,
								Shade = el.Shade,
								Faces = el.Faces.Select(face =>
								{
									return new KeyValuePair<BlockFace, BlockModelElementFace>(face.Key, new BlockModelElementFace()
									{
										Rotation = face.Value.Rotation,
										Texture = ResolveTexture(x.Model, face.Value.Texture),
										CullFace = face.Value.CullFace,
										TintIndex = face.Value.TintIndex,
										UV = face.Value.UV
									});
								}).ToDictionary(ff => ff.Key, rr => rr.Value)
							};
						}).ToArray()
					}
				};
			}).ToArray();
			
			//Models = models;
			UseRandomizer = useRandomizer;
			
			CalculateBoundingBoxes(Models);
		}

		public override BoundingBox[] GetIntersecting(Vector3 position, BoundingBox box)
		{
			List<BoundingBox> intersecting = new List<BoundingBox>();
			foreach (var b in Boxes.OrderByDescending(x => x.Max.Y))
			{
				if (new BoundingBox(position + b.Min, position + b.Max).Contains(box) == ContainmentType.Intersects)
				{
					intersecting.Add(b);
				}
			}

			return intersecting.ToArray();
		}

		private BoundingBox[] GetBoxes(Vector3 position)
		{
			return Boxes.Select(x => new BoundingBox(position + x.Min, position + x.Max)).ToArray();
		}
		
		public override BoundingBox? GetPartBoundingBox(Vector3 position, BoundingBox entityBox)
		{
			var boxes = GetBoxes(position);

			foreach (var corner in entityBox.GetCorners().OrderBy(x => x.Y))
			{
				foreach (var box in boxes.OrderByDescending(x => x.Max.Y))
				{
					var result = box.Contains(corner);
					if (result == ContainmentType.Contains || result == ContainmentType.Intersects)
					{
						return box;
					}
				}
			}
			
			foreach (var box in boxes.OrderByDescending(x => x.Max.Y))
			{
				var result = entityBox.Contains(box);
				if (result == ContainmentType.Intersects || result == ContainmentType.Contains)
				{
					return box;
				}
			}

			return null;
		}

		public override BoundingBox GetBoundingBox(Vector3 position, Block requestingBlock)
		{
			const float minThickness = 0.1f;
			Vector3 min = Min;
			Vector3 max = Max;

			var distanceX = max.X - min.X;
			if (distanceX < minThickness)
			{
				max.X += minThickness - distanceX;
			}

			var distanceZ = max.Z - min.Z;
			if (distanceZ < minThickness)
			{
				max.Z += minThickness - distanceZ;
			}
			
			var distanceY = max.Y - min.Y;
			if (distanceY < minThickness)
			{
				max.Y += minThickness - distanceY;
			}

			return new BoundingBox(position + (min), position + ((max)));
		}

		private string ResolveTexture(ResourcePackLib.Json.Models.Blocks.BlockModel var, string texture)
		{
			if (texture[0] != '#')
				return texture;

			var modified = texture.Substring(1);
			if (var.Textures.TryGetValue(modified, out texture))
			{
				if (texture[0] == '#')
				{
					if (!var.Textures.TryGetValue(texture.Substring(1), out texture))
					{
						texture = "no_texture";
					}
				}
			}

			return texture;
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
			
			//if (!world.HasBlock(pos.X, pos.Y, pos.Z)) 
			//	return false;

			var theBlock = world.GetBlockState(pos).Block;

			if (!theBlock.Renderable)
				return true;
			
			return me.ShouldRenderFace(face, theBlock);
		}
		
		protected void CalculateBoundingBoxes(BlockStateModel[] models)
		{
			for (var index = 0; index < models.Length; index++)
			{
				var model = models[index];
				ProcessModel(model, model.Model, out Vector3 min, out Vector3 max);

				if (max.X > Max.X)
					Max.X = max.X;

				if (max.Y > Max.Y)
					Max.Y = max.Y;

				if (max.Z > Max.Z)
					Max.Z = max.Z;

				if (min.X < Min.X)
					Min.X = min.X;

				if (min.Y < Min.Y)
					Min.Y = min.Y;

				if (min.Z < Min.Z)
					Min.Z = min.Z;
			}
		}

		private void FixElementScale(BlockModelElement element,
			BlockShaderVertex[] verts,
			float minX, float maxX, float minY, float maxY, float minZ, float maxZ,
			ref float facesMinX,
			ref float facesMaxX,
			ref float facesMinY,
			ref float facesMaxY,
			ref float facesMinZ,
			ref float facesMaxZ)
		{
			if (element.Rotation.Axis != Axis.Undefined && element.Rotation.Rescale)
			{
				var diffX = maxX - minX;
				var diffY = maxY - minY;
				var diffZ = maxZ - minZ;

				for (var i = 0; i < verts.Length; i++)
				{
					var v = verts[i];

					v.Position.X = (v.Position.X - minX) / diffX;
					v.Position.Y = (v.Position.Y - minY) / diffY;
					v.Position.Z = (v.Position.Z - minZ) / diffZ;

					verts[i] = v;

					if (v.Position.X < facesMinX)
					{
						facesMinX = v.Position.X;
					}
					else if (v.Position.X > facesMaxX)
					{
						facesMaxX = v.Position.X;
					}

					if (v.Position.Y < facesMinY)
					{
						facesMinY = v.Position.Y;
					}
					else if (v.Position.Y > facesMaxY)
					{
						facesMaxY = v.Position.Y;
					}

					if (v.Position.Z < facesMinZ)
					{
						facesMinZ = v.Position.Z;
					}
					else if (v.Position.Z > facesMaxZ)
					{
						facesMaxZ = v.Position.Z;
					}
				}
			}
		}

		private void ProcessModel(BlockStateModel stateModel, ResourcePackLib.Json.Models.Blocks.BlockModel model, out Vector3 min, out Vector3 max)
		{
			float facesMinX = float.MaxValue, facesMinY = float.MaxValue, facesMinZ = float.MaxValue;
			float facesMaxX = float.MinValue, facesMaxY = float.MinValue, facesMaxZ = float.MinValue;

			List<BoundingBox> boxes = new List<BoundingBox>();
			for (var index = 0; index < model.Elements.Length; index++)
			{
				var element = model.Elements[index];
				element.To *= (Scale);

				element.From *= (Scale);

				foreach (var face in element.Faces)
				{
					var verts = GetFaceVertices(face.Key, element.From, element.To, new UVMap(), out _);

					float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
					float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
					
					for (int i = 0; i < verts.Length; i++)
					{
						var v = verts[i];
						//v.Position += (v.Normal * scale);
						
						v.Position = FixRotation(v.Position, element, stateModel.X, stateModel.Y);

						v.Position /= 16f;

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

						verts[i] = v;
					}

					FixElementScale(
						element, verts, minX, maxX, minY, maxY, minZ, maxZ, ref facesMinX, ref facesMaxX, ref facesMinY,
						ref facesMaxY, ref facesMinZ, ref facesMaxZ);

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

				var from = FixRotation(element.From, element, stateModel.X, stateModel.Y);
				var to = FixRotation(element.To, element, stateModel.X, stateModel.Y);

				boxes.Add(new BoundingBox(Vector3.Min(from, to) / 16f, Vector3.Max(from, to) / 16f));
			}

			min = new Vector3(facesMinX, facesMinY, facesMinZ);
			max = new Vector3(facesMaxX, facesMaxY, facesMaxZ);

			Boxes = Boxes.Concat(boxes.ToArray()).ToArray();
		}

		private Vector3 FixRotation(Vector3 v, BlockModelElement element, int xRot, int yRot)
		{
			if (element.Rotation.Axis != Axis.Undefined)
			{
				var r = element.Rotation;
				var angle = (float) (r.Angle * (Math.PI / 180f));
				
				var origin = r.Origin;
							
				var c = MathF.Cos(angle);
				var s = MathF.Sin(angle);

				switch (r.Axis)
				{
					case Axis.Y:
					{
						var x = v.X - origin.X;
						var z = v.Z - origin.Z;

						v.X = origin.X + (x * c - z * s);
						v.Z = origin.Z + (z * c + x * s);
					}
						break;

					case Axis.X:
					{
						var x = v.Z - origin.Z;
						var z = v.Y - origin.Y;

						v.Z = origin.Z + (x * c - z * s);
						v.Y = origin.Y + (z * c + x * s);
					}
						break;

					case Axis.Z:
					{
						var x = v.X - origin.X;
						var z = v.Y - origin.Y;

						v.X = origin.X + (x * c - z * s);
						v.Y = origin.Y + (z * c + x * s);
					}
						break;
				}
			}
			
			if (xRot > 0)
			{
				var rotX = (float) (xRot * (Math.PI / 180f));
				var c = MathF.Cos(rotX);
				var s = MathF.Sin(rotX);
				var z = v.Z - 8f;
				var y = v.Y - 8f;

				v.Z = 8f + (z * c - y * s);
				v.Y = 8f + (y * c + z * s);
			}

			if (yRot > 0)
			{
				var rotX = (float) (yRot * (Math.PI / 180f));
				var c = MathF.Cos(rotX);
				var s = MathF.Sin(rotX);
				var z = v.X - 8f;
				var y = v.Z - 8f;

				v.X = 8f + (z * c - y * s);
				v.Z = 8f + (y * c + z * s);
			}

			return v;
		}

		/*private BlockCoordinates[] GetCornerOffsetsForFace(BlockFace face)
		{
			//0 = TopLeft, 1 = TopRight
			//2 = BottomLeft, 3 = BottomRight
			BlockCoordinates[] corners = new BlockCoordinates[4];

			switch (face)
			{
				case BlockFace.Down:
				case BlockFace.Up:
					corners[0] = new BlockCoordinates(-1, 0, 1);
					corners[1] = new BlockCoordinates(1, 0, 1);
					break;
			}
		}
		*/
		private void CalculateModel(IBlockAccess world,
			Vector3 position,
			Block baseBlock,
			BlockStateModel bsModel,
			IList<BlockShaderVertex> verts,
			List<int> indexResult,
			int biomeId,
			Biome biome)
		{
			var model = bsModel.Model;
			var baseColor = Color.White;

			if (biomeId != -1)
			{
				var mapColor = baseBlock.BlockMaterial.GetMaterialMapColor();
				if (mapColor == MapColor.GRASS)
				{
					baseColor = Resources.ResourcePack.GetGrassColor(
						biome.Temperature, biome.Downfall, (int) position.Y);
				}
				else if (mapColor == MapColor.FOLIAGE)
				{
					baseColor = Resources.ResourcePack.GetFoliageColor(
						biome.Temperature, biome.Downfall, (int) position.Y);
				}
			}
			
			for (var index = 0; index < model.Elements.Length; index++)
			{
				var element = model.Elements[index];
				element.To *= Scale;

				element.From *= Scale;

				foreach (var face in element.Faces)
				{
					var facing = face.Key;
					//var originalFacing = facing;
							
					var cullFace = face.Value.CullFace;

					if (cullFace == null)
						cullFace = facing;
	
					//var originalCullFace = cullFace;
					
					if (bsModel.X > 0f)
					{
						var offset = -bsModel.X / 90;
						cullFace = RotateDirection(cullFace.Value, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
						facing = RotateDirection(facing, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
					}

					if (bsModel.Y > 0f)
					{
						var offset = -bsModel.Y / 90;
						cullFace = RotateDirection(cullFace.Value, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
						facing = RotateDirection(facing, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
					}
					
					if (!ShouldRenderFace(world, cullFace.Value, position, baseBlock))
						continue;

					var textureRotation = face.Value.Rotation;
					string texture = face.Value.Texture;
					if (bsModel.Uvlock)
					{
						if (element.Faces.TryGetValue(facing, out var newRotation) && newRotation.Texture != texture)
						{
							texture = newRotation.Texture;
						}
						else
						{
							if (bsModel.X > 0)
							{
								//textureRotation = bsModel.X;
							}

							if ((face.Key == BlockFace.Up || face.Key == BlockFace.Down) && bsModel.Y > 0)
							{
								//textureRotation = bsModel.Y;
							}
						}
					}
						
					var uv = face.Value.UV;
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

					var faceColor = face.Value.TintIndex.HasValue ? baseColor : Color.White;
					var facePosition = position + cullFace.Value.GetVector3();

					var blockLight = (byte) 0;
					var skyLight = (byte) 15;
					//GetLight(
					//	world, facePosition, out blockLight, out skyLight, baseBlock.Transparent || !baseBlock.Solid);
					
					var vertices = GetFaceVertices(face.Key, element.From, element.To,
						GetTextureUVMap(Resources, texture, x1, x2, y1, y2, textureRotation, AdjustColor(
							faceColor, facing, element.Shade)),
						out int[] indexes);

					float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
					float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
					
					for (int i = 0; i < vertices.Length; i++)
					{
						var v = vertices[i];
						//v.Position += (v.Normal * scale);
						
						v.Position = FixRotation(v.Position, element, bsModel.X, bsModel.Y);

						v.Position /= 16f;

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

						vertices[i] = v;
					}
					
					FixElementScale(
						element, vertices, minX, maxX, minY, maxY, minZ, maxZ, ref minX, ref maxX, ref minY, ref maxY, ref minZ,
						ref maxZ);

					var initialIndex = verts.Count;

					byte vertexBlockLight = 0, vertexSkyLight = 0;

					if (!SmoothLighting)
					{
						GetLight(
							world, facePosition, out vertexBlockLight, out vertexSkyLight,
							baseBlock.Transparent || !baseBlock.Solid);
					}
					
					for (var idx = 0; idx < vertices.Length; idx++)
					{
						var vertex = vertices[idx];
						vertex.Position = position + vertex.Position;
						
						//var vertexSkyLight = world.GetSkyLight(blockPos);
						//var vertexBlockLight = world.GetBlockLight(blockPos);
						if (SmoothLighting)
						{
							GetLight(world, vertex.Position, out vertexBlockLight, out vertexSkyLight, true);
						}

						//if (blockLight > 0)
						{
							vertex.BlockLight = vertexBlockLight;
							vertex.SkyLight = vertexSkyLight;
						}

						verts.Add(vertex);
					}

					for (var idx = 0; idx < indexes.Length; idx++)
					{
						var idxx = indexes[idx];
						indexResult.Add(initialIndex + idxx);
					}
				}
			}
		}

		protected (BlockShaderVertex[] vertices, int[] indexes) GetVertices(IBlockAccess world,
			Vector3 position, Block baseBlock,
			BlockStateModel[] models)
		{
			var verts = new List<BlockShaderVertex>();
			{
				var indexResult = new List<int>(24 * models.Length);

				int biomeId = 0;//world == null ? 0 : world.GetBiome((int) position.X, 0, (int) position.Z);
				var biome   = BiomeUtils.GetBiomeById(biomeId);

				if (UseRandomizer)
				{
					//var rndIndex = FastRandom.Next() % Models.Length;
					CalculateModel(
						world, position, baseBlock, models[0], verts, indexResult, biomeId, biome);
				}
				else
				{
					for (var bsModelIndex = 0; bsModelIndex < models.Length; bsModelIndex++)
					{
						var bsModel = models[bsModelIndex];

						if (bsModel.Model == null) continue;

						CalculateModel(
							world, position, baseBlock, bsModel, verts, indexResult, biomeId,
							biome);
					}
				}

				return (verts.ToArray(), indexResult.ToArray());
			}
		}
		
		public override (BlockShaderVertex[] vertices, int[] indexes) GetVertices(IBlockAccess blockAccess, Vector3 position, Block baseBlock)
		{
			return GetVertices(blockAccess, position, baseBlock, Models);
		}
	}
}