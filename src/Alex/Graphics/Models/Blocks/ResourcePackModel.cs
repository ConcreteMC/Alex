using System;
using System.Collections.Generic;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using NLog;
using Axis = Alex.ResourcePackLib.Json.Axis;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using V3 = Microsoft.Xna.Framework.Vector3;

namespace Alex.Graphics.Models.Blocks
{
    public class ResourcePackModel : BlockModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourcePackModel));

		public BlockStateModel[] Variant { get; set; }
		protected ResourceManager Resources { get; }

		public ResourcePackModel(ResourceManager resources, BlockStateModel[] variant)
		{
			Resources = resources;
            Variant = variant;
        }

		protected ResourcePackModel(ResourceManager resources)
		{
			Resources = resources;
		}

		protected Matrix GetElementRotationMatrix(BlockModelElementRotation elementRotation, out float rescale)
		{
			Matrix faceRotationMatrix = Matrix.Identity;
			float ci = 0f;
			
			if (elementRotation.Axis != Axis.Undefined)
			{
				var elementRotationOrigin = new Vector3(elementRotation.Origin.X , elementRotation.Origin.Y, elementRotation.Origin.Z);

				var elementAngle = MathUtils.ToRadians((float)(360f + elementRotation.Angle) % 360f);
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

		protected void GetCullFaceValues(string facename, BlockFace originalFace, out BlockFace face, out V3 offset)
		{
			V3 cullFace = V3.Zero;
			BlockFace cull;

			switch (facename)
			{
				case "up":
					cullFace = V3.Up;
					cull = BlockFace.Up;
					break;
				case "down":
					cullFace = V3.Down;
					cull = BlockFace.Down;
					break;
				case "north":
					cullFace = V3.Backward;
					cull = BlockFace.North;
					break;
				case "south":
					cullFace = V3.Forward;
					cull = BlockFace.South;
					break;
				case "west":
					cullFace = V3.Left;
					cull = BlockFace.West;
					break;
				case "east":
					cullFace = V3.Right;
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
			return Matrix.CreateRotationX(MathUtils.ToRadians((model.X) % 360f)) *
			       Matrix.CreateRotationY(MathUtils.ToRadians((model.Y) % 360f));
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

			var block = world.GetBlockState(pos.X, pos.Y, pos.Z).Block;
			//BlockFactory.
			//var block = world.GetBlock(pos);

			if (me.Transparent && block is UnknownBlock)
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
			if (block.Solid && !block.Transparent) return false;

			return true;
		}

		protected V3 Min = V3.Zero;
		protected V3 Max = V3.One / 16f;
		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, V3 position, IBlock baseBlock)
        {
	       throw new NotImplementedException();
        }

		public override BoundingBox GetBoundingBox(V3 position, IBlock requestingBlock)
		{
			return new BoundingBox(position + Min, position + Max);
		}
	}

	public static class VectorExtension {
		public static V3 From(V3 x, V3 y, V3 z)
		{
			return new V3(x.X, y.Y, z.Z);
		}
	}
}
