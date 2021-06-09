using System;
using System.Linq;
using Alex.Common.Graphics;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.Gamestates.Login;
using Alex.Gamestates.MainMenu.Profile;
using Alex.Gui;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.Gamestates.MainMenu
{
    public class ProfileSelectionState : ListSelectionStateBase<ProfileEntry>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ProfileSelectionState));
        
        private GuiPanoramaSkyBox _skyBox;
        private IPlayerProfileService ProfileService { get; } 
        private Alex Alex { get; }

        private Button _addBtn, _editBtn, _deleteBtn, _cancelBtn, _selectBtn;
        public ProfileSelectionState(GuiPanoramaSkyBox skyBox, Alex alex)
        {
            Alex = alex;
            _skyBox = skyBox;
            ProfileService = GetService<IPlayerProfileService>();
            
            Title = "Select Profile";
            
            Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);

            //base.ListContainer.ChildAnchor = Alignment.MiddleCenter;
            //base.ListContainer.Orientation = Orientation.Horizontal;
            
            Footer.AddRow(row =>
            {
                row.AddChild(_addBtn = new AlexButton("Add", AddClicked)
                {
                    
                });
                row.AddChild(_editBtn = new AlexButton("Edit", EditClicked)
                {
                    Enabled = false
                });
                row.AddChild(_deleteBtn = new AlexButton("Delete", DeleteClicked)
                {
                    Enabled = false
                });
            });
            
            Footer.AddRow(row =>
            {
             //   row.ChildAnchor = Alignment.CenterX;
             row.AddChild(_selectBtn = new AlexButton("Select Profile", OnProfileSelect)
             {
                 Enabled = false
             });
             
                row.AddChild(_cancelBtn = new AlexButton("Cancel", OnCancelButtonPressed)
                {
                });
                
            });
            
            if (_defaultSkin == null)
            {
                Alex.Instance.Resources.TryGetBitmap("entity/alex", out var rawTexture);
                _defaultSkin = new Skin()
                {
                    Slim = true,
                    Texture = TextureUtils.BitmapToTexture2D(this, Alex.Instance.GraphicsDevice, rawTexture)
                };
            }

            Reload();
        }

        private void SetButtonState(bool itemSelected)
        {
            _selectBtn.Enabled = itemSelected;
            _deleteBtn.Enabled = itemSelected;
            _editBtn.Enabled = itemSelected;
        }
        
        private void OnProfileSelect()
        {
            var selected = SelectedItem;
            if (selected == null)
            {
                SetButtonState(false);
                return;
            }
            
            ProfileService.Force(selected.Profile);
            
            OnCancelButtonPressed();
        }

        private void Reload()
        {
            ClearItems();
            
            var activeProfile = ProfileService.CurrentProfile;
            /*foreach (var profile in ProfileService.GetJavaProfiles().Concat(ProfileService.GetBedrockProfiles()))
            {
                ProfileEntry entry = new ProfileEntry(profile, _defaultSkin, OnDoubleClick);
                AddItem(entry);

                if (activeProfile != null &&
                    profile.Uuid.Equals(activeProfile.Uuid, StringComparison.InvariantCultureIgnoreCase))
                {
                    Focus(entry);
                    _previous = entry;
                }
            }*/
        }

        private void OnDoubleClick(ProfileEntry profile)
        {
            Focus(profile);
            
            ProfileService.Force(profile.Profile);
            OnCancelButtonPressed();
        }

        private void OnBedrockConfirmed(PlayerProfile profile)
        {
            Reload();
            
            //ProfileEntry entry = new ProfileEntry(profile, _defaultSkin);
           // AddItem(entry);
        }

        private void OnJavaConfirmed()
        {
            Reload();
        }

        private void EditClicked()
        {
            
        }
        
        private void DeleteClicked()
        {
            
        }

        private void AddClicked()
        {
           // Alex.GameStateManager.SetActiveState(new VersionSelectionState(_skyBox, OnJavaConfirmed, OnBedrockConfirmed), true);
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

        private ProfileEntry _previous = null;
        protected override void OnSelectedItemChanged(ProfileEntry newItem)
        {
            base.OnSelectedItemChanged(newItem);

            if (_previous != null)
            {
                ClearFocus(_previous);
            }

            if (newItem != null)
            {
                SetButtonState(true);
            }else{
                SetButtonState(false);
            }
        }
    }
}