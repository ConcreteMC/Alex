using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Singleplayer;
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
					Model = new ResourcePackLib.Json.Models.ResourcePackModelBase()
					{
						AmbientOcclusion = x.Model.AmbientOcclusion,
						Display = null,
						Elements = x.Model.Elements.Select(el =>
						{
							return new ModelElement()
							{
								From = el.From,
								To = el.To,
								Rotation = el.Rotation,
								Shade = el.Shade,
								Faces = el.Faces.Select(face =>
								{
									return new KeyValuePair<BlockFace, ModelElementFace>(face.Key, new ModelElementFace()
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

		public override BoundingBox GetBoundingBox(Vector3 position)
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

		public static string ResolveTexture(ResourcePackLib.Json.Models.ResourcePackModelBase var, string texture)
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

		private void FixElementScale(ModelElement element,
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

		private void ProcessModel(BlockStateModel stateModel, ResourcePackLib.Json.Models.ResourcePackModelBase model, out Vector3 min, out Vector3 max)
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
						
						v.Position = FixRotation(v.Position, element);

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

				var from = FixRotation(element.From, element);
				var to   = FixRotation(element.To, element);

				boxes.Add(new BoundingBox(Vector3.Min(from, to) / 16f, Vector3.Max(from, to) / 16f));
			}

			min = new Vector3(facesMinX, facesMinY, facesMinZ);
			max = new Vector3(facesMaxX, facesMaxY, facesMaxZ);

			Boxes = Boxes.Concat(boxes.ToArray()).ToArray();
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
		
		private void CalculateModel(IBlockAccess world,
			Vector3 position,
			Block baseBlock,
			BlockStateModel bsModel,
			IList<BlockShaderVertex> verts,
			List<int> indexResult,
			List<int> animatedIndexResult,
			int biomeId,
			Biome biome)
		{
			//bsModel.Y = Math.Abs(180 - bsModel.Y);
			var model = bsModel.Model;
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

					if (bsModel.X > 0f)
					{
						var offset = bsModel.X / 90;
						cullFace = RotateDirection(cullFace, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
						facing = RotateDirection(facing, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
					}

					if (bsModel.Y > 0f)
					{
						var offset = bsModel.Y / 90;
						cullFace = RotateDirection(cullFace, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
						facing = RotateDirection(facing, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
					}
					
					if (!ShouldRenderFace(world, facing, position, baseBlock))
						continue;
					
					
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

					var faceColor = baseColor;

					if (face.Value.TintIndex.HasValue && face.Value.TintIndex == 0)
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
								faceColor = Resources.ResourcePack.GetGrassColor(
									biome.Temperature, biome.Downfall, (int) position.Y);
								break;
							case TintType.Foliage:
								faceColor = Resources.ResourcePack.GetFoliageColor(
									biome.Temperature, biome.Downfall, (int) position.Y);
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}
					
					/*switch (facing)
					{
						case BlockFace.Down:
							faceColor = Color.Black;
							break;

						case BlockFace.Up:
							faceColor = Color.White;
							break;

						case BlockFace.East:
							faceColor = Color.Orange;
							break;

						case BlockFace.West: //Correct
							faceColor = Color.Green;
							break;

						case BlockFace.North: //Correct
							faceColor = Color.Red;
							break;

						case BlockFace.South:
							faceColor = Color.Yellow;
							break;

						case BlockFace.None:
							break;
					}}*/
					faceColor = AdjustColor(faceColor, facing, element.Shade);

					var uvMap = GetTextureUVMap(
						Resources, face.Value.Texture, x1, x2, y1, y2, face.Value.Rotation,
						faceColor);
					
					var vertices = GetFaceVertices(face.Key, element.From, element.To,
						uvMap,
						out int[] indexes);

					for (int i = 0; i < vertices.Length; i++)
					{
						var v = vertices[i];
						
						v.Position /= 16f;
						v.Position = FixRotation(v.Position, element);

						if (bsModel.X > 0)
						{
							var rotX = (float) (bsModel.X * (Math.PI / 180f));
							var c    = MathF.Cos(rotX);
							var s    = MathF.Sin(rotX);
							var z    = v.Position.Z - 0.5f;
							var y    = v.Position.Y - 0.5f;

							v.Position.Z = 0.5f + (z * c - y * s);
							v.Position.Y = 0.5f + (y * c + z * s);
						}

						if (bsModel.Y > 0)
						{
							var rotY = (float) (bsModel.Y * (Math.PI / 180f));
							var c    = MathF.Cos(rotY);
							var s    = MathF.Sin(rotY);
							var x    = v.Position.X - 0.5f;
							var z    = v.Position.Z - 0.5f;

							v.Position.X = 0.5f + (x * c - z * s);
							v.Position.Z = 0.5f + (z * c + x * s);
						}
						
						var tw = uvMap.TextureInfo.Width ;
						var th = uvMap.TextureInfo.Height;
						if (face.Value.Rotation > 0)
						{
							var rotY = (float) (-face.Value.Rotation * (Math.PI / 180f));
							var c    = MathF.Cos(rotY);
							var s    = MathF.Sin(rotY);
							var x    = v.TexCoords.X - 8f * tw;
							var y    = v.TexCoords.Y - 8f * th;

							v.TexCoords.X = 8f * tw + (x * c - y * s);
							v.TexCoords.Y = 8f * th + (y * c + x * s);
						}

						if (bsModel.Uvlock)
						{
							if (bsModel.Y > 0 && (facing == BlockFace.Up || face.Key == BlockFace.Down))
							{
								var rotY = (float) (bsModel.Y * (Math.PI / 180f));
								var c    = MathF.Cos(rotY);
								var s    = MathF.Sin(rotY);
								var x    = v.TexCoords.X - 8f * tw;
								var y    = v.TexCoords.Y - 8f * th;

								v.TexCoords.X = 8f * tw + (x * c - y * s);
								v.TexCoords.Y = 8f * th + (y * c + x * s);
							}
							
							if (bsModel.X > 0 && (facing != BlockFace.Up && face.Key != BlockFace.Down))
							{
								var rotX = (float) (bsModel.X * (Math.PI / 180f));
								var c    = MathF.Cos(rotX);
								var s    = MathF.Sin(rotX);
								var x    = v.TexCoords.X - 8f * tw;
								var y    = v.TexCoords.Y - 8f * th;

								v.TexCoords.X = 8f * tw + (x * c - y * s);
								v.TexCoords.Y = 8f * th + (y * c + x * s);
							}
						}

						v.TexCoords += uvMap.TextureInfo.Position;
						v.TexCoords *= (Vector2.One / uvMap.TextureInfo.AtlasSize);

						vertices[i] = v;
					}

					var initialIndex = verts.Count;

					byte vertexBlockLight = 0, vertexSkyLight = 0;

					if (!SmoothLighting)
					{
						GetLight(
							world, position + facing.GetVector3(), out vertexBlockLight, out vertexSkyLight,
							baseBlock.Transparent || !baseBlock.Solid);
					}
					
					Vector3 lightOffset =  facing.GetVector3();
					
					for (var idx = 0; idx < vertices.Length; idx++)
					{
						var vertex = vertices[idx];
						vertex.Position = position + vertex.Position;

						if (SmoothLighting)
						{
							GetLight(world, vertex.Position + lightOffset, out vertexBlockLight, out vertexSkyLight, true);
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

						if (uvMap.IsAnimated)
						{
							animatedIndexResult.Add(initialIndex + idxx);
						}
						else
						{
							indexResult.Add(initialIndex + idxx);
						}
					}
				}
			}
		}

		protected VerticesResult GetVertices(IBlockAccess world,
			Vector3 position, Block baseBlock,
			BlockStateModel[] models)
		{
			var verts = new List<BlockShaderVertex>();
			{
				var indexResult = new List<int>(24 * models.Length);
				var animatedIndexResult = new List<int>();

				int biomeId = 0;//world == null ? 0 : world.GetBiome((int) position.X, 0, (int) position.Z);
				var biome   = BiomeUtils.GetBiomeById(biomeId);

				if (UseRandomizer)
				{
					//var rndIndex = FastRandom.Next() % Models.Length;
					CalculateModel(
						world, position, baseBlock, models[0], verts, indexResult, animatedIndexResult, biomeId, biome);
				}
				else
				{
					for (var bsModelIndex = 0; bsModelIndex < models.Length; bsModelIndex++)
					{
						var bsModel = models[bsModelIndex];

						if (bsModel.Model == null) continue;

						CalculateModel(
							world, position, baseBlock, bsModel, verts, indexResult, animatedIndexResult, biomeId,
							biome);
					}
				}

				return new VerticesResult(verts.ToArray(), indexResult.ToArray(), animatedIndexResult.Count > 0 ? animatedIndexResult.ToArray() : null);
			}
		}
		
		public override VerticesResult GetVertices(IBlockAccess blockAccess, Vector3 position, Block baseBlock)
		{
			return GetVertices(blockAccess, position, baseBlock, Models);
		}
	}
}