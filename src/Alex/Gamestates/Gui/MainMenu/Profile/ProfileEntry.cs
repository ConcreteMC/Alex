using System.Drawing;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;
using Org.BouncyCastle.Crypto.Tls;
using Color = Microsoft.Xna.Framework.Color;
using Size = Alex.API.Gui.Size;

namespace Alex.Gamestates.Gui.MainMenu.Profile
{
    public class ProfileEntry : GuiSelectionListItem
    {
        private GuiTextElement _serverName;
        public GuiEntityModelView ModelView { get; }
        public ProfileEntry(Skin defaultSkin)
        {
            MinWidth = 92;
            MaxWidth = 92;
            MinHeight = 128;
            MaxHeight = 128;
            
           // AutoSizeMode = AutoSizeMode.GrowOnly;
            
            AddChild(_serverName = new GuiTextElement()
            {
                Text = "My Profile",
                Margin = Thickness.Zero,
                Anchor = Alignment.BottomCenter,
                Enabled = false
            });

            Margin = new Thickness(0, 8);
            Anchor = Alignment.FillY;
           // AutoSizeMode = AutoSizeMode.GrowAndShrink;
           // BackgroundOverlay = new GuiTexture2D(GuiTextures.OptionsBackground);

            ModelView = new GuiEntityModelView(new PlayerMob("", null, null, defaultSkin.Texture, defaultSkin.Slim)) /*"geometry.humanoid.customSlim"*/
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
            
            var mousePos = Alex.Instance.InputManager.CursorInputListener.GetCursorPosition();

            mousePos = Vector2.Transform(mousePos, Alex.Instance.GuiManager.ScaledResolution.InverseTransformMatrix);
            var playerPos = ModelView.RenderBounds.Center.ToVector2();

            var mouseDelta = (new Vector3(playerPos.X, playerPos.Y, _playerViewDepth) - new Vector3(mousePos.X, mousePos.Y, 0.0f));
            mouseDelta.Normalize();

            var headYaw = (float)mouseDelta.GetYaw();
            var pitch = (float)mouseDelta.GetPitch();
            var yaw = (float)headYaw;

            ModelView.SetEntityRotation(-yaw, -pitch, -headYaw);
        }
    }
}