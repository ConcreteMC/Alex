using System.Linq;
using Alex.Blocks.State;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Worlds;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Items
{
	public class ItemBlockModelRenderer : ItemModelRenderer<VertexPositionColorTexture>
	{
		private BlockState _blockState;
		private Texture2D _texture;

		public ItemBlockModelRenderer(BlockState block,
			ResourcePackModelBase resourcePackModel,
			Texture2D texture,
			bool calculateSize = true,
			VertexPositionColorTexture[] vertices = null) : base(
			resourcePackModel, VertexPositionColorTexture.VertexDeclaration, vertices)
		{
			_blockState = block;
			_texture = texture;

			Scale = 16f;
			Size = new Vector3(16f, 16f, 16f);

			if (calculateSize)
			{
				float biggestDimensions = 0;

				foreach (var bb in block.Block.GetBoundingBoxes(Vector3.Zero))
				{
					var dimension = bb.GetDimensions();
					var len = dimension.Length();

					if (len > biggestDimensions)
					{
						biggestDimensions = len;
						Size = dimension;
					}
				}
			}
		}


		private bool _cached = false;

		public override bool Cache(ResourceManager pack)
		{
			if (_cached)
				return true;

			_cached = true;
			var world = new ItemRenderingWorld(_blockState.Block);

			ChunkData chunkData = new ChunkData(0, 0);

			_blockState?.VariantMapper.Model.GetVertices(world, chunkData, BlockCoordinates.Zero, _blockState);

			//pack.BlockAtlas.GetAtlasLocation(ResourcePackModel)
			var textureSize = Vector2.One;

			if (_texture != null)
			{
				textureSize = _texture.Bounds.Size.ToVector2();
			}

			var rawVertices = chunkData.BuildVertices();
			var scale = Vector2.One / textureSize;

			Vertices = rawVertices.Select(
				x => new VertexPositionColorTexture(
					x.Position, x.Color, new Vector2(x.TexCoords.X, x.TexCoords.Y) * scale)).ToArray();

			chunkData.Dispose();


			return true;
		}

		protected override void InitEffect(BasicEffect effect)
		{
			base.InitEffect(effect);
			effect.TextureEnabled = true;

			if (_texture != null)
			{
				effect.Texture = _texture;
			}
		}

		/// <inheritdoc />
		protected override void UpdateDisplayInfo(DisplayPosition displayPosition, DisplayElement displayElement)
		{
			var root = Model?.Root;

			if (root != null)
			{
				if (displayPosition.HasFlag(DisplayPosition.Gui))
				{
					root.Pivot = Vector3.Zero;
					root.BaseScale = new Vector3(displayElement.Scale.X, displayElement.Scale.Y, displayElement.Scale.Z);
					root.BaseRotation = new Vector3(25f, 45f, 0f);

					root.BasePosition = new Vector3(
						displayElement.Translation.X, displayElement.Translation.Y + 0.25f,
						displayElement.Translation.Z);
				}
				else if (displayPosition.HasFlag(DisplayPosition.ThirdPerson))
				{
					root.BaseScale = new Vector3(displayElement.Scale.X, displayElement.Scale.Y, displayElement.Scale.Z) * Scale;
					root.BaseRotation = new Vector3(-67.5f, 0f, 0f);

					root.BasePosition = new Vector3(
						displayElement.Translation.X, displayElement.Translation.Y + 8f,
						(displayElement.Translation.Z));
				}
				else if (displayPosition.HasFlag(DisplayPosition.FirstPerson))
				{
					root.BaseScale = new Vector3(displayElement.Scale.X, displayElement.Scale.Y, displayElement.Scale.Z) * (Scale / 2f);

					root.BaseRotation = new Vector3(-67.5f, 0f, 0f) + new Vector3(
						displayElement.Rotation.X, displayElement.Rotation.Y, displayElement.Rotation.Z);

					root.BasePosition = new Vector3(
						displayElement.Translation.X + 4f, displayElement.Translation.Y + 18f,
						displayElement.Translation.Z - 2f);
				}
				else if (displayPosition.HasFlag(DisplayPosition.Ground))
				{
					root.BaseScale = new Vector3(displayElement.Scale.X, displayElement.Scale.Y, displayElement.Scale.Z) * Scale;

					root.BaseRotation = new Vector3(
						displayElement.Rotation.X, displayElement.Rotation.Y, displayElement.Rotation.Z);

					root.BasePosition = new Vector3(
						displayElement.Translation.X, displayElement.Translation.Y, displayElement.Translation.Z);
				}
				else
				{
					base.UpdateDisplayInfo(displayPosition, displayElement);
				}
			}
		}

		/// <inheritdoc />
		protected override void OnDispose(bool disposing)
		{
			base.OnDispose(disposing);

			if (disposing)
			{
				var t = _texture;
				_texture = null;

				if (t != null && t.Tag != AtlasGenerator.Tag)
				{
					t?.Dispose();
				}
			}
		}
	}
}