using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.Utils;
using Alex.Worlds;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NLog;
using Axis = Alex.ResourcePackLib.Json.Axis;

namespace Alex.Graphics.Models.Blocks
{
	public class CachedResourcePackModel : BlockModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

		public BlockStateModel[] Models { get; set; }
		protected ResourceManager Resources { get; }
		private readonly IDictionary<string, FaceCache> _elementCache;

		private float Height = 1f, Width = 1f, Depth = 1f;
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
			return new BoundingBox(position, position + new Vector3(Width / 16f, Height / 16f, Depth / 16f));
			return base.GetBoundingBox(position, requestingBlock);
		}

		protected Matrix GetElementRotationMatrix(BlockModelElementRotation elementRotation, out float rescale)
		{
			Matrix faceRotationMatrix = Matrix.Identity;
			float ci = 0f;

			if (elementRotation.Axis != Axis.Undefined)
			{
				var elementRotationOrigin = new Vector3(elementRotation.Origin.X, elementRotation.Origin.Y, elementRotation.Origin.Z);

				var elementAngle = MathHelper.ToRadians((float)(elementRotation.Angle));
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

			rescale = ci;
			return faceRotationMatrix;
		}

		protected void GetCullFaceValues(string facename, BlockFace originalFace, out BlockFace face, out Vector3 offset)
		{
			Vector3 cullFace = Vector3.Zero;
			BlockFace cull;

			switch (facename)
			{
				case "up":
					cullFace = Vector3.Up;
					cull = BlockFace.Up;
					break;
				case "down":
					cullFace = Vector3.Down;
					cull = BlockFace.Down;
					break;
				case "north":
					cullFace = Vector3.Forward;
					cull = BlockFace.North;
					break;
				case "south":
					cullFace = Vector3.Backward;
					cull = BlockFace.South;
					break;
				case "west":
					cullFace = Vector3.Left;
					cull = BlockFace.West;
					break;
				case "east":
					cullFace = Vector3.Right;
					cull = BlockFace.East;
					break;
				default:
					cull = originalFace;
					cullFace = cull.GetVector3();
					break;
			}

			offset = cullFace;
			face = cull;
		}

		protected Matrix GetModelRotationMatrix(BlockStateModel model)
		{
			return Matrix.CreateRotationX(MathHelper.ToRadians(360f - model.X)) *
				   Matrix.CreateRotationY(MathHelper.ToRadians(360f - model.Y));
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

			//var block = world.GetBlockState(pos.X, pos.Y, pos.Z).Block;
			
			//.GetBlockState(pos.X, pos.Y, pos.Z).Block;
                                                                            //BlockFactory.
                                                                            //var block = world.GetBlock(pos);

          //if (me.Transparent)
          //{
		//	return true;
         // }


            // if (me.Solid && blockTransparent)
            // {
            //     return false;
            // }

            if (me.Transparent)
            {
	            return true;
            }

            var blockTransparent = world.IsTransparent(pos.X, pos.Y, pos.Z);
            var blockSolid = world.IsSolid(pos.X, pos.Y, pos.Z);

            //  world.GetBlockData(pos.X, pos.Y, pos.Z, out bool blockTransparent, out bool blockSolid);

            if (me.Solid && me.Transparent)
            {
	            //	if (IsFullCube && Name.Equals(block.Name)) return false;
	            if (blockSolid && !blockTransparent) return false;
            }
            else if (me.Transparent)
            {
	            if (blockSolid && !blockTransparent) return false;
            }


            if (me.Solid && blockTransparent) return true;
            //   if (me.Transparent && block.Transparent && !block.Solid) return false;
            if (me.Transparent) return true;
            if (!me.Transparent && blockTransparent) return true;
            if (blockSolid && !blockTransparent) return false;

            /*if (me.Transparent && block is UnknownBlock)
            {
                return true;
            }

            if (me.Solid && me.Transparent)
            {
                //	if (IsFullCube && Name.Equals(block.Name)) return false;
                if (block.Solid && !block.Transparent) return false;
            }
            else if (me.Transparent)
            {
                if (block.Solid && !block.Transparent) return false;
            }


            if (me.Solid && block.Transparent) return true;
            //   if (me.Transparent && block.Transparent && !block.Solid) return false;
            if (me.Transparent) return true;
            if (!me.Transparent && block.Transparent) return true;
            if (block.Solid && !block.Transparent) return false;*/

            return true;
		}

		protected Vector3 Min = Vector3.Zero;
		protected Vector3 Max = Vector3.One / 16f;

		protected class FaceCache
		{
		
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

		protected IDictionary<string, FaceCache> CalculateModel(BlockStateModel[] models)
		{
			Dictionary<string, FaceCache> result = new Dictionary<string, FaceCache>();
			foreach (var model in models)
			{
				if (model.Model == null) continue;
				
				for (var i = 0; i < model.Model.Elements.Length; i++)
				{
					var element = model.Model.Elements[i];
					var elementFrom = new Vector3((element.From.X), (element.From.Y),
						(element.From.Z));

					var elementTo = new Vector3((element.To.X), (element.To.Y),
						(element.To.Z));

					//elementTo = Vector3.Transform(elementTo, Matrix.CreateScale(1f / 16f));
					//elementFrom = Vector3.Transform(elementFrom, Matrix.CreateScale(1f / 16f));

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

					FaceCache elementCache = new FaceCache();
					foreach (var face in element.Faces)
					{
						VertexPositionNormalTextureColor[] faceVertices;

						var uv = face.Value.UV;

						var text = ResolveTexture(model, face.Value.Texture);

						var uvmap = GetTextureUVMap(Resources, text, uv.X1, uv.X2, uv.Y1, uv.Y2, face.Value.Rotation);

						faceVertices = GetFaceVertices(face.Key, elementFrom, elementTo, uvmap);

						float minX = 1f, minY = 1f, minZ = 1f;
						float maxX = -1f, maxY = -1f, maxZ = -1f;

						for (var index = 0; index < faceVertices.Length; index++)
						{
							var vert = faceVertices[index];

							//Apply element rotation
							if (elementRotation.Axis != Axis.Undefined)
							{
								vert.Position = Vector3.TransformNormal(vert.Position, elementRotationMatrix);

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

							vert.Position = Vector3.Transform(vert.Position, Matrix.CreateTranslation(-element.Rotation.Origin) * GetModelRotationMatrix(model) *
								Matrix.CreateTranslation(element.Rotation.Origin));

							vert.Position = Vector3.Transform(vert.Position, Matrix.CreateScale(1f / 16f));

							//Scale the position
							//vert.Position = (vert.Position / 16f);

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

						Min = Vector3.Min(new Vector3(minX, minY, minZ), Min);
						Max = Vector3.Max(new Vector3(maxX, maxY, maxZ), Max);

						elementCache.Set(face.Key, faceVertices);
					}

					if (!result.ContainsKey(model.ModelName + ":" + i))
					{
						result.Add(model.ModelName + ":" + i, elementCache);
					}
				}
			}

			return result;
		}

        protected VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, IBlock baseBlock,
			BlockStateModel[] models, IDictionary<string, FaceCache> faceCache)
		{
			var verts = new List<VertexPositionNormalTextureColor>(36);

			// MaxY = 0;
			Vector3 worldPosition = new Vector3(position.X, position.Y, position.Z);

			foreach (var model in models)
			{
				if (model.Model == null ) continue;
				
				for (var i = 0; i < model.Model.Elements.Length; i++)
				{
					FaceCache elementCache;
					if (!faceCache.TryGetValue(model.ModelName + ":" + i, out elementCache))
					{
						Log.Warn($"Element cache is null!");
						continue;
					}

					var element = model.Model.Elements[i];

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

						if (cullFace != Vector3.Zero && !ShouldRenderFace(world, cull, worldPosition, baseBlock))
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
							int biomeId = world.GetBiome((int) worldPosition.X, 0, (int) worldPosition.Z);

							if (biomeId != -1)
							{
								var biome = BiomeUtils.GetBiomeById(biomeId);

								if (baseBlock.Name.Equals("grass_block", StringComparison.InvariantCultureIgnoreCase))
								{
									faceColor = Resources.ResourcePack.GetGrassColor(biome.Temperature, biome.Downfall, (int) worldPosition.Y);
								}
								else
								{
									faceColor = Resources.ResourcePack.GetFoliageColor(biome.Temperature, biome.Downfall, (int) worldPosition.Y);
								}
							}
						}

						faceColor = LightingUtils.AdjustColor(faceColor, cull,
							GetLight(world, worldPosition + cullFace, model.Model.AmbientOcclusion), element.Shade);

						//TODO: Rotate vertices
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
			return GetVertices(world, position, baseBlock, Models, _elementCache);
		}
	}
}