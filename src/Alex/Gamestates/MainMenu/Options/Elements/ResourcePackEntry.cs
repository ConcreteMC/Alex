using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.ResourcePackLib.Generic;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options.Elements
{
    public class ResourcePackEntry : GuiSelectionListItem
    {
        public string Path { get; private set; }
        public ResourcePackManifest Manifest { get; private set; }
        
        private readonly GuiTextureElement _icon;
        private readonly GuiStackContainer _textWrapper;
        private readonly LoadIcon _loadedIcon;
        public ResourcePackEntry(ResourcePackManifest manifest, string path) : this(path, manifest.Name, manifest.Description)
        {
            if (manifest.Icon != null)
            {
                _icon.Texture = TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, manifest.Icon);
            }
        }

        public ResourcePackEntry(string path, string name, string description)
        {
            Path = path;
            Manifest = null;
            
            SetFixedSize(355, 36);
            
            Margin = new Thickness(5, 5, 5, 5);
            Padding = Thickness.One;
            Anchor = Alignment.TopFill;
            
            AddChild( _icon = new GuiTextureElement()
            {
                Width = 32,
                Height = 32,
                
                Anchor = Alignment.TopLeft,

                Background = GuiTextures.DefaultServerIcon,
                AutoSizeMode = AutoSizeMode.None,
                RepeatMode = TextureRepeatMode.NoRepeat
            });

            AddChild( _textWrapper = new GuiStackContainer()
            {
                ChildAnchor = Alignment.TopFill,
                Anchor = Alignment.TopLeft
            });
            _textWrapper.Padding = new Thickness(0,0);
            _textWrapper.Margin = new Thickness(37, 0, 0, 0);

            _textWrapper.AddChild(new GuiTextElement()
            {
                Text = name,
                Margin = Thickness.Zero
            });

            _textWrapper.AddChild(new GuiTextElement()
            {
                Text = description,
                Margin = new Thickness(0, 0, 5, 0),
				
                //Anchor = center
            });
            
            AddChild(_loadedIcon = new LoadIcon()
            {
                Anchor = Alignment.TopRight,
                AutoSizeMode = AutoSizeMode.None
            });
        }

        public bool IsLoaded => _loadedIcon.Loaded;
        public void SetLoaded(bool isLoaded)
        {
            _loadedIcon.SetLoaded(isLoaded);
        }

        private class LoadIcon : GuiImage
        {
            public bool Loaded { get; private set; }
            public LoadIcon() : base(GuiTextures.GreyCheckMark, TextureRepeatMode.NoRepeat)
            {
                SetFixedSize(15, 15);
            }

            public void SetLoaded(bool isLoaded)
            {
                if (isLoaded)
                {
                    Loaded = true;
                    Background = GuiTextures.GreenCheckMark;
                }
                else
                {
                    Loaded = false;
                    Background = GuiTextures.GreyCheckMark;
                }
            }
        }
    }
}