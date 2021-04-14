using System;
using System.Diagnostics;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Elements;

using Alex.API.Input;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
using Alex.Entities;
using Alex.Graphics.Models.Entity;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Context3D;
using Alex.ResourcePackLib;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Input;

namespace Alex.Gamestates.MainMenu.Profile
{
	public class SkinEntry : SelectionListItem
    {
        public GuiEntityModelView ModelView { get; }
        public LoadedSkin Skin { get; }
        private Action<SkinEntry> OnDoubleClick { get; }

        public SkinEntry(LoadedSkin skin, PooledTexture2D texture2D, Action<SkinEntry> onDoubleClick)
        {
            Skin = skin;
            OnDoubleClick = onDoubleClick;

            MinWidth = 92;
            MaxWidth = 92;
            MinHeight = 128;
            MaxHeight = 128;

            // AutoSizeMode = AutoSizeMode.GrowOnly;

            AddChild(
                new TextElement()
                {
                    Text = skin.Name, Margin = Thickness.Zero, Anchor = Alignment.TopCenter, //Enabled = false
                });

            Margin = new Thickness(0, 8);
            Anchor = Alignment.FillY;
            // AutoSizeMode = AutoSizeMode.GrowAndShrink;
            // BackgroundOverlay = new GuiTexture2D(GuiTextures.OptionsBackground);

            var mob = new RemotePlayer(null, null);
            
            if (EntityModelRenderer.TryGetModel(skin.Model, out var renderer))
            {
                mob.ModelRenderer = renderer;
                mob.Texture = texture2D;  
            }
            
            ModelView = new GuiEntityModelView(mob) /*"geometry.humanoid.customSlim"*/
                {
                    BackgroundOverlay = new Color(Color.Black, 0.15f),
                    Background = null,
                    //   Margin = new Thickness(15, 15, 5, 40),

                    Width = 92,
                    Height = 128,
                    Anchor = Alignment.Fill,
                };

            AddChild(ModelView);
        }

        private readonly float _playerViewDepth = -512.0f;
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
            
            var mousePos = Alex.Instance.GuiManager.FocusManager.CursorPosition;

            mousePos = Vector2.Transform(mousePos, Alex.Instance.GuiManager.ScaledResolution.InverseTransformMatrix);
            var playerPos = ModelView.RenderBounds.Center.ToVector2();

            var mouseDelta = (new Vector3(playerPos.X, playerPos.Y, _playerViewDepth) - new Vector3(mousePos.X, mousePos.Y, 0.0f));
            mouseDelta.Normalize();

            var headYaw = (float)mouseDelta.GetYaw();
            var pitch = (float)mouseDelta.GetPitch();
            var yaw = (float)headYaw;

            ModelView.SetEntityRotation(-yaw, pitch, -headYaw);
        }

        private Stopwatch _previousClick = null;
        private bool FirstClick = true;
        protected override void OnCursorPressed(Point cursorPosition, MouseButton button)
        {
            base.OnCursorPressed(cursorPosition, button);

            if (_previousClick == null)
            {
                _previousClick = Stopwatch.StartNew();
                FirstClick = false;
                return;
            }

            if (FirstClick)
            {
                _previousClick.Restart();
                FirstClick = false;
            }
            else
            {
                if (_previousClick.ElapsedMilliseconds < 150)
                {
                    OnDoubleClick?.Invoke(this);
                }

                FirstClick = true;
            }
        }
    }
}