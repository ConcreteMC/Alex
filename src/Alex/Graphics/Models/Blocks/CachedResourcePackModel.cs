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
using Microsoft.Xna.Framework;
using NLog;
using MathF = Alex.API.Utils.MathF;
using Matrix = System.Drawing.Drawing2D.Matrix;

namespace Alex.Graphics.Models.Blocks
{
	public class CachedResourcePackModel : BlockModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));
		private static FastRandom FastRandom { get; } = new FastRandom();
		
		private BlockStateModel[] Models { get; set; }
		protected ResourceManager Resources { get; }
		private IDictionary<string, FaceCache> _elementCache = null;
		
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
		public CachedResourcePackModel(ResourceManager resources, BlockStateModel[] models, bool useRandomizer = false)
		{
			Resources = resources;
			Models = models;
			UseRandomizer = useRandomizer;
			
			_elementCache = CalculateModel(Models);
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

		public override BoundingBox GetBoundingBox(Vector3 position, IBlock requestingBlock)
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

		protected void GetCullFaceValues(string facename, BlockFace facing, out BlockFace cullFace)
		{
			switch (facename.ToLower())
			{
				case "up":
					cullFace = BlockFace.Up;
					break;
				case "down":
					cullFace = BlockFace.Down;
					break;
				case "north":
					cullFace = BlockFace.North;
					break;
				case "south":
					cullFace = BlockFace.South;
					break;
				case "west":
					cullFace = BlockFace.West;
					break;
				case "east":
					cullFace = BlockFace.East;
					break;
				case "none":
					cullFace = BlockFace.None;
					break;
				default:
					cullFace = facing;
					break;
			}
		}

		protected string ResolveTexture(BlockStateModel var, string texture)
		{
			string textureName = "no_texture";
			if (!var.Model.Textures.TryGetValue(texture.Replace("#", ""), out textureName))
			{
				textureName = texture;
			}

			if (textureName.StartsWith("#"))
			{
				if (!var.Model.Textures.TryGetValue(textureName.Replace("#", ""), out textureName))
				{
					textureName = "no_texture";
				}
			}

			return textureName;
		}

		internal virtual bool ShouldRenderFace(IWorld world, BlockFace face, BlockCoordinates position, IBlock me)
		{
			if (world == null) return true;
			
			if (position.Y >= 256) return true;

			var pos = position + face.GetBlockCoordinates();

			var cX = (int)pos.X & 0xf;
			var cZ = (int)pos.Z & 0xf;

			if (cX < 0 || cX > 16)
				return false;

			if (cZ < 0 || cZ > 16)
				return false;
			
			//if (!world.HasBlock(pos.X, pos.Y, pos.Z)) 
			//	return false;

			var theBlock = world.GetBlock(pos.X, pos.Y, pos.Z);

			if (!theBlock.Renderable)
				return true;
			
			return me.ShouldRenderFace(face, theBlock);
		}
		
		protected IDictionary<string, FaceCache> CalculateModel(BlockStateModel[] models)
		{
			Dictionary<string, FaceCache> result = new Dictionary<string, FaceCache>();
			for (var index = 0; index < models.Length; index++)
			{
				var model = models[index];
				ProcessModel(model, out Vector3 min, out Vector3 max);
			//	{
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

				//	result.Add($"{index}:{r.Key}", r.Value);
			//	}
			}

			return result;
		}
		
		private void ProcessModel(BlockStateModel raw, out Vector3 min, out Vector3 max)
		{
			float facesMinX = float.MaxValue, facesMinY = float.MaxValue, facesMinZ = float.MaxValue;
			float facesMaxX = float.MinValue, facesMaxY = float.MinValue, facesMaxZ = float.MinValue;
			
		//	Dictionary<string, FaceCache> faceCaches = new Dictionary<string, FaceCache>();
				
			var model = raw.Model;

			List<BoundingBox> boxes = new List<BoundingBox>();
			for (var index = 0; index < model.Elements.Length; index++)
			{
				var element = model.Elements[index];
				element.To *= (Scale);

				element.From *= (Scale);

				//FaceCache cache = new FaceCache();

				foreach (var face in element.Faces)
				{
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

					var verts = GetFaceVertices(face.Key, element.From, element.To,
						GetTextureUVMap(Resources, ResolveTexture(raw, face.Value.Texture), x1, x2, y1, y2, face.Value.Rotation, Color.White),
						out int[] indexes);

					float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
					float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
					
					for (int i = 0; i < verts.Length; i++)
					{
						var v = verts[i];
						//v.Position += (v.Normal * scale);
						
						v.Position = FixRotation(v.Position, raw, element);

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
							else if (v.Position.X >facesMaxX)
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

					//cache.Set(face.Key, new FaceData(verts, indexes, face.Value.Rotation, null));
				}
				//faceCaches.Add(index.ToString(), cache);
				
				var from = FixRotation(element.From, raw, element);
				var to = FixRotation(element.To, raw, element);

				
				boxes.Add(new BoundingBox(Vector3.Min(from, to) / 16f, Vector3.Max(from, to) / 16f));
			}

			min = new Vector3(facesMinX, facesMinY, facesMinZ);
			max = new Vector3(facesMaxX, facesMaxY, facesMaxZ);

			Boxes = Boxes.Concat(boxes.ToArray()).ToArray();
			
			//return faceCaches;
		}

		private Vector3 FixRotation(Vector3 v, BlockStateModel raw, BlockModelElement element)
		{
			if (element.Rotation.Axis != Axis.Undefined)
			{
				var r = element.Rotation;
				var angle = (float) (r.Angle * (Math.PI / 180f));
							
				//angle = r.Axis == Axis.Z ? angle : -angle;
				//angle = r.Axis == Axis.Y ? -angle : angle;
							
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
			
			if (raw.X > 0)
			{
				var rotX = (float) (raw.X * (Math.PI / 180f));
				var c = MathF.Cos(rotX);
				var s = MathF.Sin(rotX);
				var z = v.Z - 8f;
				var y = v.Y - 8f;

				v.Z = 8f + (z * c - y * s);
				v.Y = 8f + (y * c + z * s);
			}

			if (raw.Y > 0)
			{
				var rotX = (float) (raw.Y * (Math.PI / 180f));
				var c = MathF.Cos(rotX);
				var s = MathF.Sin(rotX);
				var z = v.X - 8f;
				var y = v.Z - 8f;

				v.X = 8f + (z * c - y * s);
				v.Z = 8f + (y * c + z * s);
			}

			return v;
		}

		private void CalculateModel(IWorld world,
			Vector3 position,
			IBlock baseBlock,
			BlockStateModel bsModel,
			List<VertexPositionNormalTextureColor> verts,
			List<int> indexResult,
			IDictionary<string, FaceCache> faceCache,
			int bsModelIndex,
			int biomeId,
			Biome biome)
		{
			var model = bsModel.Model;
			var baseColor = Color.White;

			if (biomeId != -1)
			{
				if (baseBlock.Name.Equals("grass_block", StringComparison.InvariantCultureIgnoreCase))
				{
					baseColor = Resources.ResourcePack.GetGrassColor(
						biome.Temperature, biome.Downfall, (int) position.Y);
				}
				else
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
					if (bsModel.X > 0f)
					{
						var offset = -bsModel.X / 90;
						//cullFace = RotateDirection(cullFace, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
						facing = RotateDirection(facing, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
					}

					if (bsModel.Y > 0f)
					{
						var offset = -bsModel.Y / 90;
						//cullFace = RotateDirection(cullFace, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
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

					var faceColor = face.Value.TintIndex.HasValue ? baseColor : Color.White;

					int lighting = world == null ? 15 : GetLight(
						world, position, position + facing.GetVector3(), baseBlock.Transparent || !baseBlock.Solid);
					
					var vertices = GetFaceVertices(face.Key, element.From, element.To,
						GetTextureUVMap(Resources, ResolveTexture(bsModel, face.Value.Texture), x1, x2, y1, y2, face.Value.Rotation, AdjustColor(
							faceColor, facing, lighting, element.Shade)),
						out int[] indexes);

					float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
					float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
					
					for (int i = 0; i < vertices.Length; i++)
					{
						var v = vertices[i];
						//v.Position += (v.Normal * scale);
						
						v.Position = FixRotation(v.Position, bsModel, element);

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
					
					if (element.Rotation.Axis != Axis.Undefined && element.Rotation.Rescale)
					{
						var diffX = maxX - minX;
						var diffY = maxY - minY;
						var diffZ = maxZ - minZ;

						for (var i = 0; i < vertices.Length; i++)
						{
							var v = vertices[i];
							
							v.Position.X = (v.Position.X - minX) / diffX;
							v.Position.Y = (v.Position.Y - minY) / diffY;
							v.Position.Z = (v.Position.Z - minZ) / diffZ;
							
							vertices[i] = v;
						}
					}

					//cache.Set(face.Key, new FaceData(verts, indexes, face.Value.Rotation, null));
					
					var initialIndex = verts.Count;

					for (var idx = 0; idx < vertices.Length; idx++)
					{
						var vertex = vertices[idx];
						vertex.Position = position + vertex.Position;

						verts.Add(vertex);
					}

					for (var idx = 0; idx < indexes.Length; idx++)
					{
						var idxx = indexes[idx];
						indexResult.Add(initialIndex + idxx);
					}
				}
				//faceCaches.Add(index.ToString(), cache);
				
				//var from = FixRotation(element.From, bsModel, element);
				//var to = FixRotation(element.To, bsModel, element);
			}
			
			/*var model         = bsModel.Model;
			var modelElements = model.Elements;

			for (var i = 0; i < modelElements.Length; i++)
			{
				FaceCache elementCache;

				if (!faceCache.TryGetValue($"{bsModelIndex}:{i}", out elementCache))
				{
					Log.Warn($"Element cache is null!");

					continue;
				}

				var scale = 0f;

				var element = modelElements[i];

				//var otherElements = modelElements.Where(e => e != element).ToArray();

				foreach (var faceElement in element.Faces)
				{
					var facing = faceElement.Key;

					GetCullFaceValues(faceElement.Value.CullFace, facing, out var cullFace);

					var originalCullFace = cullFace;

					if (bsModel.X > 0f)
					{
						var offset = -bsModel.X / 90;
						//cullFace = RotateDirection(cullFace, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
						facing = RotateDirection(facing, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
					}

					if (bsModel.Y > 0f)
					{
						var offset = -bsModel.Y / 90;
						//cullFace = RotateDirection(cullFace, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
						facing = RotateDirection(facing, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
					}

					if (originalCullFace != BlockFace.None && !ShouldRenderFace(world, facing, position, baseBlock))
						continue;


					FaceData faceVertices;

					if (!elementCache.TryGet(faceElement.Key, out faceVertices) || faceVertices.Vertices.Length == 0
					                                                            || faceVertices.Indexes.Length == 0)
					{
						//Log.Debug($"No vertices cached for face {faceElement.Key} in model {bsModel.ModelName}");
						continue;
					}

					Color faceColor = faceVertices.Vertices[0].Color;

					if (faceElement.Value.TintIndex.HasValue)
					{
						if (biomeId != -1)
						{
							if (baseBlock.Name.Equals("grass_block", StringComparison.InvariantCultureIgnoreCase))
							{
								faceColor = Resources.ResourcePack.GetGrassColor(
									biome.Temperature, biome.Downfall, (int) position.Y);
							}
							else
							{
								faceColor = Resources.ResourcePack.GetFoliageColor(
									biome.Temperature, biome.Downfall, (int) position.Y);
							}
						}
					}


					faceColor = AdjustColor(
						faceColor, facing,
						world == null ? 15 : (GetLight(
							world, position, position + facing.GetVector3(),
							baseBlock.Transparent || !baseBlock.Solid)), element.Shade);


					var s            = (facing.GetVector3() * scale);
					var initialIndex = verts.Count;

					for (var index = 0; index < faceVertices.Vertices.Length; index++)
					{
						var vertex = faceVertices.Vertices[index];
						vertex.Color = faceColor;
						vertex.Position = position + vertex.Position + s;

						verts.Add(vertex);
					}

					for (var index = 0; index < faceVertices.Indexes.Length; index++)
					{
						var idx = faceVertices.Indexes[index];
						indexResult.Add(initialIndex + idx);
					}
				}
			}*/
		}

		protected (VertexPositionNormalTextureColor[] vertices, int[] indexes) GetVertices(IWorld world,
			Vector3 position, IBlock baseBlock,
			BlockStateModel[] models,  IDictionary<string, FaceCache> faceCache)
		{
			var verts = new List<VertexPositionNormalTextureColor>(36);
			var indexResult = new List<int>(24);

			int biomeId = world == null ? 0 : world.GetBiome((int) position.X, 0, (int) position.Z);
			var biome = BiomeUtils.GetBiomeById(biomeId);

			if (UseRandomizer)
			{
				//var rndIndex = FastRandom.Next() % Models.Length;
				CalculateModel(world, position, baseBlock, models[0], verts, indexResult, faceCache, 0,
					biomeId, biome);
			}
			else
			{
				for (var bsModelIndex = 0; bsModelIndex < models.Length; bsModelIndex++)
				{
					var bsModel = models[bsModelIndex];
					if (bsModel.Model == null) continue;

					CalculateModel(world, position, baseBlock, bsModel, verts, indexResult, faceCache, bsModelIndex,
						biomeId, biome);
				}
			}

			return (verts.ToArray(), indexResult.ToArray());
		}
		
		public override (VertexPositionNormalTextureColor[] vertices, int[] indexes) GetVertices(IWorld world, Vector3 position, IBlock baseBlock)
		{
			/*if (_elementCache == null)
			{
				_elementCache = CalculateModel(Models);
			}*/
			
			return GetVertices(world, position, baseBlock, Models, _elementCache);
		}
		
		protected class FaceCache
		{
			private Dictionary<BlockFace, FaceData> _cache = new Dictionary<BlockFace, FaceData>();
			public bool TryGet(BlockFace face, out FaceData vertices)
			{
				return _cache.TryGetValue(face, out vertices);
			}

			public void Set(BlockFace face, FaceData vertices)
			{
				_cache[face] = vertices;
			}
		}

        protected class FaceData
        {
            public VertexPositionNormalTextureColor[] Vertices { get; set; }
            public int[] Indexes { get; set; }
            public int Rotation { get; set; }
            public TexturePosition[] TexturePositions { get; set; }


            public FaceData(VertexPositionNormalTextureColor[] vertices, int[] indices, int rotation, TexturePosition[] texturePositions)
            {
                Vertices = vertices;
                Indexes = indices;
                Rotation = rotation;
            }

            public enum TexturePosition
            {
                TopLeft,
                BottomLeft,
                TopRight,
                BottomRight
            }
        }
	}
}