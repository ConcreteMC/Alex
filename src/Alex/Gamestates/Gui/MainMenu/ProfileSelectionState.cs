using System.Drawing;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.GameStates;
using Alex.GameStates.Gui.Common;
using Alex.Gamestates.Gui.MainMenu.Profile;
using Alex.Gui;
using Microsoft.Xna.Framework;

namespace Alex.Gamestates.Gui.MainMenu
{
    public class ProfileSelectionState : ListSelectionStateBase<ProfileEntry>
    {
        private GuiPanoramaSkyBox _skyBox;
        public ProfileSelectionState(GuiPanoramaSkyBox skyBox)
        {
            _skyBox = skyBox;
            
            Title = "Select Profile (Unfinished)";
            
            Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);

            base.ListContainer.ChildAnchor = Alignment.MiddleCenter;
            base.ListContainer.Orientation = Orientation.Horizontal;
            
            Footer.AddRow(row =>
            {
                row.AddChild(new GuiButton("Add", AddClicked)
                {

                });
                row.AddChild(new GuiButton("Cancel", OnCancelButtonPressed)
                {
                  //  TranslationKey = "gui.cancel"
                });
            });
            
            Footer.AddRow(row =>
            {
                row.ChildAnchor = Alignment.CenterX;
                row.AddChild(new GuiButton("Edit", EditClicked)
                {
                    Enabled = false
                });
                
            });
            
            if (_defaultSkin == null)
            {
                Alex.Instance.Resources.ResourcePack.TryGetBitmap("entity/alex", out Bitmap rawTexture);
                _defaultSkin = new Skin()
                {
                    Slim = true,
                    Texture = TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, rawTexture)
                };
            }
        }

        private void EditClicked()
        {
            
        }

        private void AddClicked()
        {
            ProfileEntry entry = new ProfileEntry(_defaultSkin);
            AddItem(entry);
        }

        private void OnCancelButtonPressed()
        {
            Alex.GameStateManager.Back();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
            _skyBox.Update(gameTime);
        }

        protected override void OnDraw(IRenderArgs args)
        {
            if (!_skyBox.Loaded)
            {
                _skyBox.Load(Alex.GuiRenderer);
            }

            _skyBox.Draw(args);
            
            base.OnDraw(args);
        }

        private Skin _defaultSkin = null;
        protected override void OnLoad(IRenderArgs args)
        {
            

            base.OnLoad(args);
        }
    }
}