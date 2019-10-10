using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using NLog;
using WinApi.User32;
using MathF = Alex.API.Utils.MathF;
using Matrix = System.Drawing.Drawing2D.Matrix;

namespace Alex.Graphics.Models.Blocks
{
	public class CachedResourcePackModel : BlockModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

		public BlockStateModel[] Models { get; set; }
		protected ResourceManager Resources { get; }
		private readonly IDictionary<string, FaceCache> _elementCache;
		
		protected Vector3 Min = new Vector3(float.MaxValue);
		protected Vector3 Max = new Vector3(float.MinValue);
		public CachedResourcePackModel(ResourceManager resources, BlockStateModel[] models)
		{
			Resources = resources;
			Models = models;

			if (models != null)
			{
				_elementCache = CalculateModel(models);
			}
		}

		public override BoundingBox GetBoundingBox(Vector3 position, IBlock requestingBlock)
		{
			return new BoundingBox(position + (Min), position + ((Max)));
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
			
			world.GetBlockData(pos.X, pos.Y, pos.Z, out bool blockTransparent, out bool blockSolid);

			if (me.Solid && me.Transparent)
			{
				//	if (IsFullCube && Name.Equals(block.Name)) return false;
				if (blockSolid && !blockTransparent) return false;
			}
			else if (me.Transparent)
			{
				if (blockSolid && !blockTransparent) return false;
				//if (blockTransparent) return true;
			}


			if (me.Solid && blockTransparent) return true;
			//   if (me.Transparent && block.Transparent && !block.Solid) return false;
			if (me.Transparent) return true;
			if (!me.Transparent && blockTransparent) return true;
			if (blockSolid && !blockTransparent) return false;
			if (me.Solid && blockSolid) return false;
			
			return true;
		}
		
		protected IDictionary<string, FaceCache> CalculateModel(BlockStateModel[] models)
		{
			Dictionary<string, FaceCache> result = new Dictionary<string, FaceCache>();
			for (var index = 0; index < models.Length; index++)
			{
				var model = models[index];
				foreach (var r in ProcessModel(model, out Vector3 min, out Vector3 max))
				{
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

					result.Add($"{model.ModelName}:{index}:{r.Key}", r.Value);
				}
			}

			return result;
		}
		
		private Dictionary<string, FaceCache> ProcessModel(BlockStateModel raw, out Vector3 min, out Vector3 max)
		{
			float facesMinX = float.MaxValue, facesMinY = float.MaxValue, facesMinZ = float.MaxValue;
			float facesMaxX = float.MinValue, facesMaxY = float.MinValue, facesMaxZ = float.MinValue;
			
			Dictionary<string, FaceCache> faceCaches = new Dictionary<string, FaceCache>();
				
			var model = raw.Model;

			for (var index = 0; index < model.Elements.Length; index++)
			{
				var element = model.Elements[index];
				element.To *= Scale;
				element.From *= Scale;
				
				FaceCache cache = new FaceCache();

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
						GetTextureUVMap(Resources, ResolveTexture(raw, face.Value.Texture), x1, x2, y1, y2, face.Value.Rotation),
						out int[] indexes);

					float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
					float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
					
					for (int i = 0; i < verts.Length; i++)
					{
						var v = verts[i];
						
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
									var x = v.Position.X - origin.X;
									var z = v.Position.Z - origin.Z;

									v.Position.X = origin.X + (x * c - z * s);
									v.Position.Z = origin.Z + (z * c + x * s);
								}
									break;

								case Axis.X:
								{
									var x = v.Position.Z - origin.Z;
									var z = v.Position.Y - origin.Y;

									v.Position.Z = origin.Z + (x * c - z * s);
									v.Position.Y = origin.Y + (z * c + x * s);
								}
									break;

								case Axis.Z:
								{
									var x = v.Position.X - origin.X;
									var z = v.Position.Y - origin.Y;

									v.Position.X = origin.X + (x * c - z * s);
									v.Position.Y = origin.Y + (z * c + x * s);
								}
									break;
							}
						}

						if (raw.X > 0)
						{
							var rotX = (float) (raw.X * (Math.PI / 180f));
							var c = MathF.Cos(rotX);
							var s = MathF.Sin(rotX);
							var z = v.Position.Z - 8f;
							var y = v.Position.Y - 8f;

							v.Position.Z = 8f + (z * c - y * s);
							v.Position.Y = 8f + (y * c + z * s);
						}

						if (raw.Y > 0)
						{
							var rotX = (float) (raw.Y * (Math.PI / 180f));
							var c = MathF.Cos(rotX);
							var s = MathF.Sin(rotX);
							var z = v.Position.X - 8f;
							var y = v.Position.Z - 8f;

							v.Position.X = 8f + (z * c - y * s);
							v.Position.Z = 8f + (y * c + z * s);
						}
						
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

					cache.Set(face.Key, new FaceData(verts, indexes, face.Value.Rotation, null));
				}
				faceCaches.Add(index.ToString(), cache);
			}

			min = new Vector3(facesMinX, facesMinY, facesMinZ);
			max = new Vector3(facesMaxX, facesMaxY, facesMaxZ);

			return faceCaches;
		}

		protected (VertexPositionNormalTextureColor[] vertices, int[] indexes) GetVertices(IWorld world, Vector3 position, IBlock baseBlock,
			BlockStateModel[] models, IDictionary<string, FaceCache> faceCache)
		{
			var verts = new List<VertexPositionNormalTextureColor>(36);
			var indexResult = new List<int>();

			int biomeId = world == null ? 0 : world.GetBiome((int) position.X, 0, (int) position.Z);
			var biome = BiomeUtils.GetBiomeById(biomeId);

			for (var bsModelIndex = 0; bsModelIndex < models.Length; bsModelIndex++)
			{
				var bsModel = models[bsModelIndex];
				if (bsModel.Model == null) continue;

				var model = bsModel.Model;
				var modelElements = model.Elements;
				for (var i = 0; i < modelElements.Length; i++)
				{
					FaceCache elementCache;
					if (!faceCache.TryGetValue($"{bsModel.ModelName}:{bsModelIndex}:{i}", out elementCache))
					{
						Log.Warn($"Element cache is null!");
						continue;
					}

					var element = modelElements[i];

					foreach (var faceElement in element.Faces)
					{
						var facing = faceElement.Key;
						
						GetCullFaceValues(faceElement.Value.CullFace, facing, out var cullFace);

						var originalCullFace = cullFace;
						
						if (bsModel.X > 0f)
						{
							var offset = (-bsModel.X) / 90;
							cullFace = RotateDirection(cullFace, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
							facing = RotateDirection(facing, offset, FACE_ROTATION_X, INVALID_FACE_ROTATION_X);
						}

						if (bsModel.Y > 0f)
						{
							var offset = (-bsModel.Y) / 90;
							cullFace = RotateDirection(cullFace, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
							facing = RotateDirection(facing, offset, FACE_ROTATION, INVALID_FACE_ROTATION);
						}

						if (originalCullFace != BlockFace.None && !ShouldRenderFace(world, facing, position, baseBlock))
							continue;


                        FaceData faceVertices;
						if (!elementCache.TryGet(faceElement.Key, out faceVertices) || faceVertices.Vertices.Length == 0 || faceVertices.Indexes.Length ==0)
						{
							//Log.Debug($"No vertices cached for face {faceElement.Key} in model {bsModel.ModelName}");
							continue;
						}

						Color faceColor = faceVertices.Vertices[0].Color;

						if (faceElement.Value.TintIndex >= 0)
						{
							if (biomeId != -1)
							{
								if (baseBlock.Name.Equals("grass_block", StringComparison.InvariantCultureIgnoreCase))
								{
									faceColor = Resources.ResourcePack.GetGrassColor(biome.Temperature, biome.Downfall,
										(int) position.Y);
								}
								else
								{
									faceColor = Resources.ResourcePack.GetFoliageColor(biome.Temperature,
										biome.Downfall, (int) position.Y);
								}
							}
						}
						
						faceColor = AdjustColor(faceColor, facing,
							world == null ? 15 : GetLight(world, position + cullFace.GetVector3(),
								false), element.Shade);

                        var initialIndex = verts.Count;
						for (var index = 0; index < faceVertices.Vertices.Length; index++)
						{
							var vertex = faceVertices.Vertices[index];
							vertex.Color = faceColor;
							vertex.Position = position + vertex.Position;

							verts.Add(vertex);
						}
						
						for (var index = 0; index < faceVertices.Indexes.Length; index++)
						{
							var idx = faceVertices.Indexes[index];
							indexResult.Add(initialIndex + idx);
						}
					}
				}
			}

			
			return (verts.ToArray(), indexResult.ToArray());
		}

		public override (VertexPositionNormalTextureColor[] vertices, int[] indexes) GetVertices(IWorld world, Vector3 position, IBlock baseBlock)
		{
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