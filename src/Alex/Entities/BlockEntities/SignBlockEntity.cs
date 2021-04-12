using System;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.BlockEntities;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;

namespace Alex.Entities.BlockEntities
{
	public class SignBlockEntity : BlockEntity
	{
		private EntityModelRenderer.ModelBone RootBone { get; set; }

		/// <inheritdoc />
		public SignBlockEntity(World level, Block block) : base(level, block)
		{
			//ModelRenderer = new EntityModelRenderer(new StandingSignEntityModel(), texture);
			
			Width = 1f;
			Height = 1f;
			
				//	Offset = Vector3.Zero;
		}

		/// <inheritdoc />
		protected override void UpdateModelParts()
		{
			base.UpdateModelParts();

			if (ModelRenderer != null && ModelRenderer.GetBone("root", out var bone))
			{
				var rot = bone.Rotation;
				rot.Y = _yRotation;
				bone.Rotation = rot;
				
				RootBone = bone;
			}
		}

		private string[] _lines = new string[4];

		public string Text1
		{
			get => _lines[0];
			set
			{
				if (ChatObject.TryParse(value, out var t1))
				{
					value = t1;
				}
				
				_lines[0] = value;
				TextChanged();
			}
		}

		public string Text2
		{
			get => _lines[1];
			set
			{
				if (ChatObject.TryParse(value, out var t1))
				{
					value = t1;
				}
				
				_lines[1] = value;
				TextChanged();
			}
		}
		public string Text3
		{
			get => _lines[2];
			set
			{
				if (ChatObject.TryParse(value, out var t1))
				{
					value = t1;
				}
				
				_lines[2] = value;
				TextChanged();
			}
		}
		
		public string Text4
		{
			get => _lines[3];
			set
			{
				if (ChatObject.TryParse(value, out var t1))
				{
					value = t1;
				}
				
				_lines[3] = value;
				TextChanged();
			}
		}
		
		private byte  _rotation  = 0;
		private float _yRotation = 0f;
		public byte Rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				_rotation = Math.Clamp(value, (byte)0, (byte)15);
				
				_yRotation          = _rotation * -22.5f;
				if (RootBone != null)
				{
					var headRotation = RootBone.Rotation;
					headRotation.Y = _yRotation;
					RootBone.Rotation = headRotation;
				}
				//HeadBone.Rotation = headRotation;
			}
		}

		private float TextOffset = 0.1f;
		protected override void BlockChanged(Block oldBlock, Block newBlock)
		{
			if (newBlock is WallSign)
			{
				ModelRenderer = new EntityModelRenderer(new WallSignEntityModel());
				Texture = BlockEntityFactory.SignTexture;
				if (newBlock.BlockState.TryGetValue("facing", out var facing))
				{
					if (Enum.TryParse<BlockFace>(facing, true, out var face))
					{
						TextOffset = 0.4f;
						switch (face)
						{
							case BlockFace.West:
								Rotation = 4;
								break;

							case BlockFace.East:
								Rotation = 12;
								break;

							case BlockFace.North:
								Rotation = 8;
								break;

							case BlockFace.South:
								Rotation = 0;
								break;
						}
					}
				}
			}
			else if (newBlock is StandingSign)
			{
				ModelRenderer = new EntityModelRenderer(new StandingSignEntityModel());
				Texture = BlockEntityFactory.SignTexture;
				TextOffset = -0.1f;
				if (newBlock.BlockState.TryGetValue("rotation", out var r))
				{
					if (byte.TryParse(r, out var rot))
					{
						Rotation = (byte) rot;// // ((rot + 3) % 15);
					}
				}
			}
		}
		
		/// <inheritdoc />
		protected override void ReadFrom(NbtCompound compound)
		{
			base.ReadFrom(compound);
			
			if (compound.TryGet("text1", out var text1)
			|| compound.TryGet("Text1", out text1))
			{
				if (text1 != null && text1.HasValue)
				{
					Text1 = text1.StringValue;
				}
			}
			
			if (compound.TryGet("text2", out var text2)
			    || compound.TryGet("Text2", out text2))
			{
				if (text2 != null && text2.HasValue)
				{
					Text2 = text2.StringValue;
				}
			}
			
			if (compound.TryGet("text3", out var text3)
			    || compound.TryGet("Text3", out text3))
			{
				if (text3 != null && text3.HasValue)
				{
					Text3 = text3.StringValue;
				}
			}
			
			if (compound.TryGet("text4", out var text4)
			    || compound.TryGet("Text4", out text4))
			{
				if (text4 != null && text4.HasValue)
				{
					Text4 = text4.StringValue;
				}
			}
			//Text1 = compound["Text1"].StringValue;
			//Text2 = compound["Text2"].StringValue;
			//Text3 = compound["Text3"].StringValue;
			//Text4 = compound["Text4"].StringValue;
		}

		private void TextChanged()
		{
			var text1 = Text1;
			var text2 = Text2;
			var text3 = Text3;
			var text4 = Text4;

			NameTag = $"{text1}\n{text2}\n{text3}\n{text4}";

			if (_lines.All(string.IsNullOrWhiteSpace))
				HideNameTag = true;
			else
			{
				HideNameTag = false;
			}
		}

		private BasicEffect _basicEffect = null;

		/// <inheritdoc />
		public override void Render2D(IRenderArgs args)
		{
			var sb = args.SpriteBatch;

			if (_basicEffect == null)
			{
				_basicEffect = new BasicEffect(args.GraphicsDevice);
				_basicEffect.FogEnabled = false;
				_basicEffect.LightingEnabled = false;
				_basicEffect.VertexColorEnabled = true;
				_basicEffect.TextureEnabled = true;
			}
			
			_basicEffect.Projection = args.Camera.ProjectionMatrix;
			_basicEffect.View = args.Camera.ViewMatrix;

			string clean = NameTag;
			if (string.IsNullOrWhiteSpace(clean))
				return;
			
			var maxDistance = (args.Camera.FarDistance) / (64f);
			
			Vector3 lookAtOffset = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(MathUtils.ToRadians(_yRotation)));
			lookAtOffset *= TextOffset;
			
			var pos = this.RenderLocation + new Vector3(lookAtOffset.X, 0.75f, lookAtOffset.Z);

			var world = Matrix.CreateScale(1f / 96f) * Matrix.CreateRotationY(MathUtils.ToRadians(_yRotation))
			                                         * Matrix.CreateTranslation(pos);

			world.Up = -world.Up;

			_basicEffect.World = world;
		
			var distance = Vector3.Distance(pos, args.Camera.Position);
			if (distance >= maxDistance)
			{
				return;
			}

			try
			{
				sb.End();
			
				sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap,
					DepthStencilState.DepthRead, effect: _basicEffect);
				
				Vector2 renderPosition = Vector2.Zero;
				int     yOffset        = 0;

				foreach (var str in clean.Split('\n'))
				{
					var line = str.Trim();

					var stringCenter = Alex.Font.MeasureString(line);

					var c = new Point((int) stringCenter.X, (int) stringCenter.Y);

					renderPosition.X = (int) -(c.X * 0.5f);
					renderPosition.Y = (int) yOffset;

					//renderArgs.SpriteBatch.FillRectangle(
					//	new Rectangle(renderPosition.ToPoint(), c), new Color(Color.Black, 128), screenSpace.Z);

					Alex.Font.DrawString(sb, line, renderPosition, (Color) TextColor.Black, FontStyle.None, Vector2.One);

					yOffset += c.Y;
				}
			}
			finally
			{
				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap,
					DepthStencilState.DepthRead);
			}
		}
	}
}