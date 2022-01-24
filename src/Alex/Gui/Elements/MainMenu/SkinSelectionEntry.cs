using System;
using System.Diagnostics;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.Gui.Elements.Context3D;
using Alex.ResourcePackLib.Bedrock;
using Alex.Utils;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Input;

namespace Alex.Gui.Elements.MainMenu
{
	public class SkinSelectionEntry : SelectionListItem
	{
		private GuiEntityModelView ModelView { get; }
		public LoadedSkin Skin { get; }
		private Action<SkinSelectionEntry> OnDoubleClick { get; }

		public SkinSelectionEntry(LoadedSkin skin, Action<SkinSelectionEntry> onDoubleClick)
		{
			Skin = skin;
			OnDoubleClick = onDoubleClick;

			MinWidth = 92;
			MaxWidth = 92;
			MinHeight = 96;
			MaxHeight = 96;

			Margin = new Thickness(0, 4);
			Anchor = Alignment.FillY;

			var mob = new RemotePlayer(null, null);

			if (skin.Model.TryGetRenderer(out var renderer))
			{
				mob.ModelRenderer = renderer;
				TextureUtils.BitmapToTexture2DAsync(this, Alex.Instance.GraphicsDevice, skin.Texture, texture =>
				{
					mob.Texture = texture;
				});
			}

			ModelView = new GuiEntityModelView(mob)
			{
				BackgroundOverlay = new Color(Color.Black, 0.15f),
				Background = null,
				Width = 92,
				Height = 96,
				Anchor = Alignment.Fill,
			};

			AddChild(ModelView);

			AddChild(new TextElement() { Text = skin.Name, Margin = Thickness.Zero, Anchor = Alignment.BottomCenter });
		}

		private readonly float _playerViewDepth = -512.0f;

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			var mousePos = Alex.Instance.GuiManager.FocusManager.CursorPosition;

			mousePos = Alex.Instance.GuiRenderer.Unproject(mousePos);
			var playerPos = ModelView.RenderBounds.Center.ToVector2();

			var mouseDelta = (new Vector3(playerPos.X, playerPos.Y, _playerViewDepth)
			                  - new Vector3(mousePos.X, mousePos.Y, 0.0f));

			mouseDelta.Normalize();

			var headYaw = (float)mouseDelta.GetYaw();
			var pitch = (float)mouseDelta.GetPitch();
			var yaw = (float)headYaw;

			ModelView.SetEntityRotation(-yaw, pitch, -headYaw);
		}

		private Stopwatch _previousClick = null;
		private bool _firstClick = true;

		protected override void OnCursorPressed(Point cursorPosition, MouseButton button)
		{
			base.OnCursorPressed(cursorPosition, button);

			if (_previousClick == null)
			{
				_previousClick = Stopwatch.StartNew();
				_firstClick = false;

				return;
			}

			if (_firstClick)
			{
				_previousClick.Restart();
				_firstClick = false;
			}
			else
			{
				if (_previousClick.ElapsedMilliseconds < 150)
				{
					OnDoubleClick?.Invoke(this);
				}

				_firstClick = true;
			}
		}
	}
}