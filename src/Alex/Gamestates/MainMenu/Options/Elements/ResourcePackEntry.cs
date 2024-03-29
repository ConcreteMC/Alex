using Alex.Common.Gui.Graphics;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.ResourcePackLib.Generic;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options.Elements
{
	public class ResourcePackEntry : ListItem
	{
		public string Path { get; private set; }
		public ResourcePackManifest Manifest { get; private set; }

		private readonly TextureElement _icon;
		private readonly StackContainer _textWrapper;
		private readonly LoadIcon _loadedIcon;

		public ResourcePackEntry(ResourcePackManifest manifest, string path) : this(
			path, manifest.Name, manifest.Description)
		{
			if (manifest.Icon != null)
			{
				TextureUtils.BitmapToTexture2DAsync(this, Alex.Instance.GraphicsDevice, manifest.Icon, texture =>
				{
					_icon.Texture = texture;
				});
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

			AddChild(
				_icon = new TextureElement()
				{
					Width = 32,
					Height = 32,
					Anchor = Alignment.TopLeft,
					Background = AlexGuiTextures.DefaultServerIcon,
					AutoSizeMode = AutoSizeMode.None,
					RepeatMode = TextureRepeatMode.NoRepeat
				});

			AddChild(
				_textWrapper = new StackContainer() { ChildAnchor = Alignment.TopFill, Anchor = Alignment.TopLeft });

			_textWrapper.Padding = new Thickness(0, 0);
			_textWrapper.Margin = new Thickness(37, 0, 0, 0);

			_textWrapper.AddChild(new TextElement() { Text = name, Margin = Thickness.Zero });

			_textWrapper.AddChild(
				new TextElement()
				{
					Text = description, Margin = new Thickness(0, 0, 5, 0),

					//Anchor = center
				});

			AddChild(_loadedIcon = new LoadIcon() { Anchor = Alignment.TopRight, AutoSizeMode = AutoSizeMode.None });
		}

		public bool IsLoaded => _loadedIcon.Loaded;

		public void SetLoaded(bool isLoaded)
		{
			_loadedIcon.SetLoaded(isLoaded);
		}

		private class LoadIcon : Image
		{
			public bool Loaded { get; private set; }

			public LoadIcon() : base(AlexGuiTextures.GreyCheckMark, TextureRepeatMode.NoRepeat)
			{
				SetFixedSize(15, 15);
			}

			public void SetLoaded(bool isLoaded)
			{
				if (isLoaded)
				{
					Loaded = true;
					Background = AlexGuiTextures.GreenCheckMark;
				}
				else
				{
					Loaded = false;
					Background = AlexGuiTextures.GreyCheckMark;
				}
			}
		}
	}
}