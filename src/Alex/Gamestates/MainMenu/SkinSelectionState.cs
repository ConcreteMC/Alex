using System.IO;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using Alex.Gamestates.MainMenu.Profile;
using Alex.Gui;
using Alex.ResourcePackLib;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;
using SixLabors.ImageSharp.Formats.Png;

namespace Alex.Gamestates.MainMenu
{
    public class SkinSelectionState : ListSelectionStateBase<SkinEntry>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SkinSelectionState));
        
        private GuiPanoramaSkyBox _skyBox;
        private IStorageSystem Storage { get; } 
        private Alex Alex { get; }

        private GuiButton _cancelBtn, _selectBtn;
        public SkinSelectionState(GuiPanoramaSkyBox skyBox, Alex alex)
        {
            Alex = alex;
            _skyBox = skyBox;
            Storage = GetService<IStorageSystem>();
            
            Title = "Select Skin";
            
            Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);

            base.ListContainer.ChildAnchor = Alignment.MiddleCenter;
            base.ListContainer.Orientation = Orientation.Horizontal;

            Footer.AddRow(row =>
            {
             //   row.ChildAnchor = Alignment.CenterX;
             row.AddChild(_selectBtn = new GuiButton("Select Skin", OnSkinSelect)
             {
                 Enabled = false
             });
             
                row.AddChild(_cancelBtn = new GuiButton("Cancel", OnCancelButtonPressed)
                {
                });
                
            });
            
            Footer.AddRow(row =>
            {
                row.ChildAnchor = Alignment.BottomCenter;
                row.AddChild(new GuiButton("Open SkinPack folder", OpenSkinPackFOlder, false)
                {
                    Modern = true
                });
            });

            Reload();
        }

        private void OpenSkinPackFOlder()
        {
            CrossPlatformUtils.OpenFolder(Alex.Resources.SkinPackDirectory.ToString());
        }

        private void SetButtonState(bool itemSelected)
        {
            _selectBtn.Enabled = itemSelected;
        }
        
        private void OnSkinSelect()
        {
            var selected = SelectedItem;
            if (selected == null)
            {
                SetButtonState(false);
                return;
            }
            
            OnDoubleClick(SelectedItem);
          //  ProfileService.Force(selected.Profile);
            
            OnCancelButtonPressed();
        }

        private void Reload()
        {
            ClearItems();

            foreach (var skinPack in Alex.Resources.Packs)
            {
                foreach (var module in skinPack.Modules.Where(x => x is MCSkinPack).Cast<MCSkinPack>())
                {
                    foreach (var skin in module.Skins)
                    {
                        Alex.UIThreadQueue.Enqueue(
                            () =>
                            {
                                var texture = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, skin.Texture);
                                SkinEntry entry = new SkinEntry(skin, texture, OnDoubleClick);
                                AddItem(entry);
                            });
                    }
                }
            }
        }

        private void OnDoubleClick(SkinEntry profile)
        {
            Focus(profile);

            Storage.TryWriteJson("skin.json", profile.Skin.Model);

            using (MemoryStream ms = new MemoryStream())
            {
                profile.Skin.Texture.Save(ms, new PngEncoder());

                Storage.TryWriteBytes("skin.png", ms.ToArray());
            }

            Alex.PlayerModel = profile.Skin.Model;
            Alex.PlayerTexture = profile.Skin.Texture;

            OnCancelButtonPressed();
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
        
        protected override void OnLoad(IRenderArgs args)
        {
            base.OnLoad(args);
        }

        private SkinEntry _previous = null;
        protected override void OnSelectedItemChanged(SkinEntry newItem)
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