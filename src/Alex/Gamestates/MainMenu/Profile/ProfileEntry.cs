using System;
using System.Diagnostics;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;
using RocketUI;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.Gamestates.MainMenu.Profile
{
    public class ProfileEntry : GuiSelectionListItem
    {
        public GuiEntityModelView ModelView { get; }
        public PlayerProfile Profile { get; }
        private Action<ProfileEntry> OnDoubleClick { get; }
        public ProfileEntry(PlayerProfile profile, Skin defaultSelection, Action<ProfileEntry> onDoubleClick)
        {
            Profile = profile;
            OnDoubleClick = onDoubleClick;
            
            MinWidth = 92;
            MaxWidth = 92;
            MinHeight = 128;
            MaxHeight = 128;
            
           // AutoSizeMode = AutoSizeMode.GrowOnly;
            
            AddChild(new GuiTextElement()
            {
                Text = profile.Username,
                Margin = Thickness.Zero,
                Anchor = Alignment.TopCenter,
                Enabled = false
            });

            Margin = new Thickness(0, 8);
            Anchor = Alignment.FillY;
           // AutoSizeMode = AutoSizeMode.GrowAndShrink;
           // BackgroundOverlay = new GuiTexture2D(GuiTextures.OptionsBackground);

            ModelView = new GuiEntityModelView(new PlayerMob(profile.Username, null, null, profile.Skin?.Texture ?? defaultSelection.Texture, (profile.Skin?.Slim ?? defaultSelection.Slim) ? "geometry.humanoid.customSlim" : "geometry.humanoid.custom")) /*"geometry.humanoid.customSlim"*/
            {
                BackgroundOverlay = new Color(Color.Black, 0.15f),
                Background = null,
             //   Margin = new Thickness(15, 15, 5, 40),

                Width = 92,
                Height = 128,

                Anchor = Alignment.Fill,
                
            };
            
            AddChild(ModelView);

            AddChild(new GuiTextElement()
            {
                Text = profile.IsBedrock ? "Bedrock" : "Java",
                Margin = Thickness.Zero,
                Anchor = Alignment.BottomCenter,
                Enabled = false,
                BackgroundOverlay = new Color(Color.Black, 0.5f),
                Background = null
            });
        }
        
        private readonly float _playerViewDepth = -512.0f;
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
            
            var mousePos = Alex.Instance.InputManager.CursorInputListener.GetCursorPosition();

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
        protected override void OnCursorPressed(Point cursorPosition)
        {
            base.OnCursorPressed(cursorPosition);

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