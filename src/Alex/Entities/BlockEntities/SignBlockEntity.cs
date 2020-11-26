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
using MathF = Alex.API.Utils.MathF;

namespace Alex.Entities.BlockEntities
{
	public class SignBlockEntity : BlockEntity
	{
		private EntityModelRenderer.ModelBone RootBone { get; set; }

		/// <inheritdoc />
		public SignBlockEntity(World level, Block block) : base(level, block)
		{
			//ModelRenderer = new EntityModelRenderer(new StandingSignEntityModel(), texture);
			
			Width = 16;
			Height = 16;
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
					value = t1.RawMessage;
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
					value = t1.RawMessage;
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
					value = t1.RawMessage;
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
					value = t1.RawMessage;
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
				
				_yRotation          = _rotation * 22.5f;
				if (RootBone != null)
				{
					var headRotation = RootBone.Rotation;
					headRotation.Y = _yRotation;
					RootBone.Rotation = headRotation;
				}
				//HeadBone.Rotation = headRotation;
			}
		}

		private Vector3 TextOffset = Vector3.Zero;
		protected override void BlockChanged(Block oldBlock, Block newBlock)
		{
			if (newBlock is WallSign)
			{
				ModelRenderer = new EntityModelRenderer(new WallSignEntityModel(), BlockEntityFactory.SignTexture);
				
				if (newBlock.BlockState.TryGetValue("facing", out var facing))
				{
					if (Enum.TryParse<BlockFace>(facing, true, out var face))
					{
						Offset = (face.Opposite().GetVector3() * 0.5f);
						TextOffset = face.GetVector3() * 0.2f;
						
						if (MathF.Abs(Offset.X) > 0f)
						{
							Offset = new Vector3(Offset.X, Offset.Y, -0.5f);
						}
						else if (MathF.Abs(Offset.Z) > 0f)
						{
							Offset = new Vector3(-0.5f, Offset.Y, Offset.Z);
						}
						
						switch (face)
						{
							case BlockFace.East:
								Rotation = 12;
								break;

							case BlockFace.West:
								Rotation = 4;
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
				ModelRenderer = new EntityModelRenderer(new StandingSignEntityModel(), BlockEntityFactory.SignTexture);
				
				if (newBlock.BlockState.TryGetValue("rotation", out var r))
				{
					if (byte.TryParse(r, out var rot))
					{
						Rotation = rot;
					}
				}
			}
		}
		
		/// <inheritdoc />
		protected override void ReadFrom(NbtCompound compound)
		{
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

		/// <inheritdoc />
		public override void RenderNametag(IRenderArgs renderArgs)
		{
						string clean = NameTag;

			if (string.IsNullOrWhiteSpace(clean))
				return;
			
			var maxDistance = (renderArgs.Camera.FarDistance) / (64f);

			var pos = KnownPosition + new Vector3(0f, 0.75f, 0f) + TextOffset;
			//pos.Y = 0;

			//var rotation = RootBone.Rotation.Y;
			
			var distance = Vector3.Distance(pos, renderArgs.Camera.Position);
			if (distance >= maxDistance)
			{
				return;
			}

			Vector2 textPosition;
			
			//var matrix = Matrix.CreateBillboard(quadPosition, cameraPosition, Vector3.Up, pForward);
			
			//Matrix rotationMatrix = Matrix.CreateRotationY(MathUtils.ToRadians(RootBone.Rotation.Y)); //Yaw

		//	Vector3 lookAtOffset = Vector3.Transform(Vector3.Backward, rotationMatrix);
			//Direction = lookAtOffset;

		//	var pos = Position + Vector3.Transform(Offset, Matrix.CreateRotationY(-Rotation.Y));
	        
		//	var target = pos + lookAtOffset;

			var screenSpace = renderArgs.GraphicsDevice.Viewport.Project(pos, 
				renderArgs.Camera.ProjectionMatrix,
				renderArgs.Camera.ViewMatrix,
				Matrix.Identity);

			textPosition.X = screenSpace.X;
			textPosition.Y = screenSpace.Y;

			Vector2 renderPosition = textPosition;
			int yOffset = 0;
			foreach (var str in clean.Split('\n'))
			{
				var line = str.Trim();
				var stringCenter = Alex.Font.MeasureString(line);
				var c            = new Point((int) stringCenter.X, (int) stringCenter.Y);

				renderPosition.X = (int) (textPosition.X - (c.X / 2d));
				renderPosition.Y = (int) (textPosition.Y - (c.Y / 2d)) + yOffset;

				//renderArgs.SpriteBatch.FillRectangle(
				//	new Rectangle(renderPosition.ToPoint(), c), new Color(Color.Black, 128), screenSpace.Z);

				Alex.Font.DrawString(
					renderArgs.SpriteBatch, line, renderPosition, TextColor.Black, FontStyle.None, Vector2.One,
					layerDepth: screenSpace.Z);

				yOffset += c.Y;
			}
		}
		
		private Vector3 Offset { get; set; } = Vector3.Zero;
		/// <inheritdoc />
		public override PlayerLocation KnownPosition
		{
			get => base.KnownPosition + Offset;
			set => base.KnownPosition = value;
		}
	}
}