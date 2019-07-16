using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Utils;
using Alex.GameStates.Gui.MainMenu.Options.Elements;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Generic;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.GameStates.Gui.MainMenu.Options
{
    public class ResourcePackOptionsState : OptionsStateBase
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourcePackOptionsState));
        
        protected ResourcePackEntry[] Items => _items.ToArray();
        private List<ResourcePackEntry> _items { get; } = new List<ResourcePackEntry>();

        protected ResourcePackEntry SelectedItem => ListContainer.SelectedItem as ResourcePackEntry;
        
        protected readonly GuiSelectionList ListContainer;

        private GuiButton _loadBtn;
        public ResourcePackOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
        {
            TitleTranslationKey = "resourcePack.title";
            
            Body.BackgroundOverlay = new Color(Color.Black, 0.35f);
            
            AddGuiElement(ListContainer = new GuiSelectionList()
            {
                Anchor = Alignment.Fill,
                ChildAnchor = Alignment.TopFill,
            });
            ListContainer.SelectedItemChanged += HandleSelectedItemChanged;

            var footerChildren = Footer.ChildElements.ToArray();
            foreach (var child in footerChildren)
            {
                Footer.RemoveChild(child);
            }

            Footer.AddRow(row =>
            {
                row.AddChild(_loadBtn = new GuiButton(LoadBtnClicked)
                {
                    Text = "Load Resource pack",
                    Modern = false,
                    Enabled = false
                });
                
                row.AddChild(new GuiBackButton()
                {
                    TranslationKey = "gui.done",
                    Modern = false
                });
            });

            Footer.AddRow(row =>
            {
                row.ChildAnchor = Alignment.BottomCenter;
                row.AddChild(new GuiButton("resourcePack.openFolder", OpenResourcePackFolderClicked, true)
                {
                    Modern = false
                });
            });

            Reload();
        }

        private void OpenResourcePackFolderClicked()
        {
            CrossPlatformUtils.OpenFolder(Alex.Resources.ResourcePackDirectory.ToString());
        }

        private void LoadBtnClicked()
        {
            var selected = SelectedItem;
            if (selected == null || !selected.Enabled)
            {
                _loadBtn.Enabled = false;
                return;
            }

            if (!selected.IsLoaded)
            {
                selected.SetLoaded(true);
                _loadBtn.Enabled = false;
            }
        }

        private void HandleSelectedItemChanged(object sender, GuiSelectionListItem item)
        {
            if (item != null && item.Enabled)
            {
                _loadBtn.Enabled = true;
            }
            else
            {
                _loadBtn.Enabled = false;
            }
        }

        private void Reload()
        {
            ClearItems();

            var stockItem = new ResourcePackEntry(null, "Default", "For those who dig the vanilla look")
            {
                Enabled = false
            };
            stockItem.SetLoaded(true);
            
            AddItem(stockItem);

            var enabled = base.Options.ResourceOptions.LoadedResourcesPacks.Value;
            
            foreach (var resource in Alex.Resources.ResourcePackDirectory.EnumerateFiles())
            {
                try
                {
                    if (resource.FullName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (Alex.Resources.TryLoadResourcePackInfo(resource.FullName,
                            out ResourcePackManifest packInfo))
                        {
                            var item = new ResourcePackEntry(packInfo, resource.FullName);

                            if (enabled.Any(x => x.ToLower().Contains(resource.Name.ToLower())))
                            {
                                item.SetLoaded(true);
                            }

                            AddItem(item);
                        }
                    }
                }
                catch (InvalidResourcePackException ex)
                {
                    Log.Warn($"Error while loading resourcepack: {ex.ToString()}");
                }
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            Reload();
        }

        public void AddItem(ResourcePackEntry item)
        {
            _items.Add(item);
            ListContainer.AddChild(item);
        }
        
        public void RemoveItem(ResourcePackEntry item)
        {
            ListContainer.RemoveChild(item);
            _items.Remove(item);
        }
        
        public void ClearItems()
        {
            foreach (var item in _items)
            {
                ListContainer.RemoveChild(item);
            }
            _items.Clear();
        }
    }
}
