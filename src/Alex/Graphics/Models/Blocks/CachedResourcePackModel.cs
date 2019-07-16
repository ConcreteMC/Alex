using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Graphics.Models.Blocks
{
	public class CachedResourcePackModel : BlockModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

		public BlockStateModel[] Models { get; set; }
		protected ResourceManager Resources { get; }
		private readonly IDictionary<string, FaceCache> _elementCache;

		private float Height = 1f, Width = 1f, Depth = 1f;
		public CachedResourcePackModel(ResourceManager resources, BlockStateModel[] models, BlockStateVariant variant)
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
			//return new BoundingBox(position, position + new Vector3(Width / 16f, Height / 16f, Depth / 16f));
			return base.GetBoundingBox(position, requestingBlock);
		}

		protected Matrix GetElementRotationMatrix(BlockModelElementRotation elementRotation, out float rescale)
		{
			if (elementRotation.Axis == Axis.Undefined)
			{
				rescale = 1f;
				return Matrix.Identity;
			}

			Matrix faceRotationMatrix = Matrix.Identity;
			
			var elementRotationOrigin =
				elementRotation
					.Origin; // new Vector3(elementRotation.Origin.X, elementRotation.Origin.Y, elementRotation.Origin.Z);

			var elementAngle = MathHelper.ToRadians((float) (elementRotation.Angle));
			
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
			
			rescale = 1f / (float) Math.Cos(elementAngle);;
			return faceRotationMatrix;
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

		protected Matrix GetModelRotationMatrix(BlockStateModel model)
		{
			return Matrix.CreateRotationX(MathHelper.ToRadians(-model.X)) *
			       Matrix.CreateRotationY(MathHelper.ToRadians(-model.Y));
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

		protected Vector3 Min = Vector3.Zero;
		protected Vector3 Max = Vector3.One / 16f;

		private static int BlockFaceLength = Enum.GetValues(typeof(BlockFace)).Length;
		protected IDictionary<string, FaceCache> CalculateModel(BlockStateModel[] models)
		{
			Dictionary<string, FaceCache> result = new Dictionary<string, FaceCache>();
			for (var bsModelIndex = 0; bsModelIndex < models.Length; bsModelIndex++)
			{
				var bsModel = models[bsModelIndex];
				var model = bsModel.Model;
				
				if (model == null) continue;
				
				bool isFlat = !model.Name.Contains("rail") && model.Elements.Sum(x => x.Faces.Count) == 2 && model.Elements.Length == 1;
				if (model.Name.Contains("vine"))
				{
					isFlat = true;
				}
				
				var modelRot = GetModelRotationMatrix(bsModel);
				
				bool isCross = false;
				/*if (model.Parent != null && model.ParentName.Contains("cross"))
				{
					isCross = true;
				}*/
				
				var modelElements = model.Elements;

				/*for (var index = 0; index < model.Elements.Length; index++)
				{
					var element = model.Elements[index];
					foreach (var key in element.Faces.Keys)
					{
						var face = element.Faces[key];
						switch (key)
						{
							case BlockFace.Down:
								
								break;
							case BlockFace.Up:
								break;
							case BlockFace.East:
							case BlockFace.West:
							case BlockFace.North:
							case BlockFace.South:
								//modelElements[index].Faces[key].Rotation = 360 - face.Rotation;
								break;
							case BlockFace.None:
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}
				}*/

				float minX = 1f, minY = 1f, minZ = 1f;
				float maxX = -1f, maxY = -1f, maxZ = -1f;
				for (var i = 0; i < modelElements.Length; i++)
				{
					var element = modelElements[i];
					var elementFrom = new Vector3((element.From.X), (element.From.Y),
						(element.From.Z));

					var elementTo = new Vector3((element.To.X), (element.To.Y),
						(element.To.Z));

					var width = elementTo.X - elementFrom.X;
					var height = elementTo.Y - elementFrom.Y;
					var depth = elementTo.Z - elementFrom.Z;

					if (width > Width)
						Width = width;

					if (height > Height)
						Height = height;

					if (depth > Depth)
						Depth = depth;

					var elementRotation = element.Rotation;
					Matrix elementRotationMatrix = GetElementRotationMatrix(elementRotation, out float scalingFactor);

					BlockFace sideToFlip = BlockFace.West;
					/*if (bsModel.Y == 0)
					{
						sideToFlip = BlockFace.West;
					}
					else if (bsModel.Y == 180)
					{
						sideToFlip = BlockFace.West;
					}
					else if (bsModel.Y == 270)
					{
						sideToFlip = BlockFace.West;
					}
					else if (bsModel.Y == 90)
					{
						sideToFlip = BlockFace.West;
					}*/

					var elementFaces = new Dictionary<BlockFace, BlockModelElementFace>(element.Faces);
					
					
					FaceCache elementCache = new FaceCache();
					foreach (var face in element.Faces)
					{
						var uv = face.Value.UV;

						BlockModelElementFace opposite;
						string text = face.Value.Texture;
						var rotation = face.Value.Rotation;

						var useFlat = (elementFaces.Count == 2 && isFlat);
						var elementCrossed = element.Rotation.Angle == 45 && element.Rotation.Axis == Axis.Y && (model.Parent == null || model.Parent.Name.Contains("cross"));
						var useCrossRendering = ((elementCrossed && useFlat) || (elementCrossed));
						
						switch (face.Key)
						{
							case BlockFace.East:
								if (elementFaces.TryGetValue(BlockFace.West, out opposite))
								{
									text = opposite.Texture;
								}
								break;
							case BlockFace.West:
								if (elementFaces.TryGetValue(BlockFace.East, out opposite))
								{
									text = opposite.Texture;
								}
								break;
							case BlockFace.North:
								if (elementFaces.TryGetValue(BlockFace.South, out opposite))
								{
									text = opposite.Texture;
								}
								break;
							case BlockFace.South:
								if (elementFaces.TryGetValue(BlockFace.North, out opposite))
								{
									text = opposite.Texture;
								}
								break;
						}
						
						text = ResolveTexture(bsModel, text);
						
						if (((face.Key == BlockFace.Up) || face.Key == BlockFace.East) && bsModel.ModelName.Contains("piston") && !useCrossRendering && !useFlat)
						{
							if (rotation == 90)
							{
								rotation = 270;
							}
							else if (rotation == 180)
							{
								rotation = 0;
							}
							else if (rotation == 270)
							{
								rotation = 90;
							}
							else if (rotation == 0)
							{
								rotation = 180;
							}
						}
						
						var uvmap = GetTextureUVMap(Resources, text, uv.X1, uv.X2, uv.Y1, uv.Y2, rotation);
						
						int[] indexes;
						var faceVertices = useCrossRendering
							? GetQuadVertices(face.Key, elementFrom, elementTo, uvmap, out indexes)
							: ( useFlat
								? GetFlatVertices(face.Key, elementFrom, elementTo, uvmap, out indexes)
								: GetFaceVertices(face.Key, elementFrom, elementTo, uvmap, out indexes));
						
						if (faceVertices == null) continue;

                        for (var index = 0; index < faceVertices.Length; index++)
						{
							var vert = faceVertices[index];

							//Apply element rotation
							if (!elementCrossed && elementRotation.Axis != Axis.Undefined)
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
							
							var rotationOrigin = new Vector3(8,8,8);
							
							vert.Position = Vector3.Transform(vert.Position,
								Matrix.CreateTranslation(-rotationOrigin) * modelRot *
								Matrix.CreateTranslation(rotationOrigin));

							vert.Position = Vector3.Transform(vert.Position, Matrix.CreateScale(1f / 16f));
							
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
						
						elementCache.Set(face.Key, new FaceData(faceVertices, indexes, rotation, null));
					}

					if (!result.ContainsKey($"{bsModel.ModelName}:{bsModelIndex}:{i}"))
					{
						result.Add($"{bsModel.ModelName}:{bsModelIndex}:{i}", elementCache);
					}
					
					Min.X = minX;
					Min.Y = minY;
					Min.Z = minZ;
					
					Max.X = maxX;
					Max.Y = maxY;
					Max.Z = maxZ;
				}
			}

			return result;
		}

		protected (VertexPositionNormalTextureColor[] vertices, int[] indexes) GetVertices(IWorld world, Vector3 position, IBlock baseBlock,
			BlockStateModel[] models, IDictionary<string, FaceCache> faceCache)
		{
			var verts = new List<VertexPositionNormalTextureColor>(36);
			var indexResult = new List<int>();
			// MaxY = 0;
			//Vector3 worldPosition = position;// new Vector3(position.X, position.Y, position.Z);
			int biomeId = world.GetBiome((int) position.X, 0, (int) position.Z);
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

						/*switch (faceElement.Key)
						{
							case BlockFace.Down:
								faceColor = Color.Turquoise;
								break;
							case BlockFace.Up:
								faceColor = Color.Blue;
								break;
							case BlockFace.East:
								faceColor = Color.Red;
								break;
							case BlockFace.West:
								faceColor = Color.Yellow;
								break;
							case BlockFace.North:
								faceColor = Color.Pink;
								break;
							case BlockFace.South:
								faceColor = Color.LimeGreen;
								break;
							case BlockFace.None:
								break;
						}*/
						
						faceColor = AdjustColor(faceColor, facing,
							GetLight(world, position + facing.GetVector3(),
								false /*model.Model.AmbientOcclusion*/), element.Shade);

						//TODO: Rotate vertices
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
							/*var vertex = faceVertices.vertices[idx];

							vertex.Color = faceColor;
							vertex.Position = position + vertex.Position;
							verts.Add(vertex);*/
							
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