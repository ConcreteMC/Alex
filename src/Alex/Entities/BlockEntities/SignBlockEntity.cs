using System.Linq;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.BlockEntities;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework;

namespace Alex.Entities.BlockEntities
{
	public class SignBlockEntity : BlockEntity
	{
		/// <inheritdoc />
		public SignBlockEntity(World level, Block block, PooledTexture2D texture) : base(level, block)
		{
			ModelRenderer = new EntityModelRenderer(new StandingSignEntityModel(), texture);
		}

		private string[] _lines = new string[4];

		public string Text1
		{
			get => _lines[0];
			set
			{
				_lines[0] = value;
				TextChanged();
			}
		}

		public string Text2
		{
			get => _lines[1];
			set
			{
				_lines[1] = value;
				TextChanged();
			}
		}
		public string Text3
		{
			get => _lines[2];
			set
			{
				_lines[2] = value;
				TextChanged();
			}
		}
		
		public string Text4
		{
			get => _lines[3];
			set
			{
				_lines[3] = value;
				TextChanged();
			}
		}

		/// <inheritdoc />
		protected override void ReadFrom(NbtCompound compound)
		{
			Text1 = compound["Text1"].StringValue;
			Text2 = compound["Text2"].StringValue;
			Text3 = compound["Text3"].StringValue;
			Text4 = compound["Text4"].StringValue;
		}

		private void TextChanged()
		{
			NameTag = $"{Text1}\n{Text2}\n{Text3}\n{Text4}";

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

			Vector3 posOffset = new Vector3(0, 0.75f, 0);

			var cameraPosition = new Vector3(renderArgs.Camera.Position.X, 0, renderArgs.Camera.Position.Z);
			
			var rotation = new Vector3(KnownPosition.X, 0, KnownPosition.Z) - cameraPosition;
			rotation.Normalize();
			
			
			var pos = KnownPosition + posOffset + (rotation);
			//pos.Y = 0;
			
			var distance = Vector3.Distance(pos, renderArgs.Camera.Position);
			if (distance >= maxDistance)
			{
				return;
			}

			Vector2 textPosition;
			
			//var matrix = Matrix.CreateBillboard(quadPosition, cameraPosition, Vector3.Up, pForward);

			var screenSpace = renderArgs.GraphicsDevice.Viewport.Project(pos, 
				renderArgs.Camera.ProjectionMatrix,
				renderArgs.Camera.ViewMatrix,
				Matrix.Identity);

			textPosition.X = screenSpace.X;
			textPosition.Y = screenSpace.Y;

			float depth = screenSpace.Z;

			var scaleRatio = (1.0f / depth);
			//var scaleRatio = Alex.Instance.GuiRenderer.ScaledResolution.ScaleFactor;
			//scale = 0.5f;
			float scaler = NametagScale - (distance * (NametagScale / maxDistance));
			//float scaler = NametagScale;
			var scale = new Vector2(scaler * scaleRatio, scaler * scaleRatio);
			//scale *= Alex.Instance.GuiRenderer.ScaledResolution.ElementScale;

			Vector2 renderPosition = textPosition;
			int yOffset = 0;
			foreach (var str in clean.Split('\n'))
			{
				var line = str.Trim();
				var stringCenter = Alex.Font.MeasureString(line, scale);
				var c            = new Point((int) stringCenter.X, (int) stringCenter.Y);

				renderPosition.X = (int) (textPosition.X - (c.X / 2d));
				renderPosition.Y = (int) (textPosition.Y - (c.Y / 2d)) + yOffset;

				//renderArgs.SpriteBatch.FillRectangle(
				//	new Rectangle(renderPosition.ToPoint(), c), new Color(Color.Black, 128), screenSpace.Z);

				Alex.Font.DrawString(
					renderArgs.SpriteBatch, line, renderPosition, TextColor.White, FontStyle.None, scale,
					layerDepth: screenSpace.Z);

				yOffset += c.Y;
			}
		}
	}
}