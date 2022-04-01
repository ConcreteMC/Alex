using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alex.Common.Data.Options;
using Alex.Common.Gui.Elements;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.Gamestates.MainMenu.Options.Elements;
using Alex.Gui;
using Alex.ResourcePackLib.Exceptions;
using Alex.ResourcePackLib.Generic;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options
{
	public class ResourcePackOptionsState : ListSelectionStateBase<ResourcePackEntry>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourcePackOptionsState));

		private Button _loadBtn;
		public ResourcePackOptionsState(GuiPanoramaSkyBox skyBox) : base()
		{
			Background = new GuiTexture2D(skyBox, TextureRepeatMode.Stretch);

			TitleTranslationKey = "resourcePack.title";

			Body.BackgroundOverlay = new Color(Color.Black, 0.35f);
			Body.ChildAnchor = Alignment.FillCenter;

			/*AddRocketElement(ListContainer = new SelectionList()
			{
			    Anchor = Alignment.Fill,
			    ChildAnchor = Alignment.TopFill,
			});
			ListContainer.SelectedItemChanged += HandleSelectedItemChanged;*/


			var footerChildren = Footer.ChildElements.ToArray();

			foreach (var child in footerChildren)
			{
				Footer.RemoveChild(child);
			}

			Footer.AddRow(
				row =>
				{
					row.AddChild(
						_loadBtn = new AlexButton(LoadBtnClicked) { Text = "Load Resource pack", Enabled = false }
						   .ApplyModernStyle(false));

					row.AddChild(
						new AlexButton(BackButtonPressed) { TranslationKey = "gui.done", }.ApplyModernStyle(false));
				});

			Footer.AddRow(
				row =>
				{
					row.ChildAnchor = Alignment.BottomCenter;

					row.AddChild(
						new AlexButton("resourcePack.openFolder", OpenResourcePackFolderClicked, true).ApplyModernStyle(
							false));
				});

			Reload();
		}

		private void BackButtonPressed()
		{
			Alex.GameStateManager.Back();

			var newItems = _newLoaded.ToArray();
			bool changed = _originallyLoaded.Length != newItems.Length;

			if (!changed)
			{
				for (int i = 0; i < _originallyLoaded.Length; i++)
				{
					if (_originallyLoaded[i] != newItems[i])
					{
						changed = true;

						break;
					}
				}
			}

			if (changed)
			{
				Options.ResourceOptions.LoadedResourcesPacks.Value = _newLoaded.ToArray();
			}
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
				if (!_newLoaded.Contains(selected.Path))
				{
					_newLoaded.Add(selected.Path);
				}

				selected.SetLoaded(true);
				_loadBtn.Enabled = false;
			}
			else
			{
				_newLoaded.Remove(selected.Path);

				selected.SetLoaded(false);
				_loadBtn.Enabled = false;
			}
		}

		protected override void OnSelectedItemChanged(ResourcePackEntry newItem)
		{
			if (newItem != null && newItem.Enabled)
			{
				_loadBtn.Enabled = true;
			}
			else
			{
				_loadBtn.Enabled = false;
			}
		}

		private string[] _originallyLoaded;
		private List<string> _newLoaded;

		private void Reload()
		{
			ClearItems();

			var stockItem =
				new ResourcePackEntry(null, "Default", "For those who dig the vanilla look") { Enabled = false };

			stockItem.SetLoaded(true);

			AddItem(stockItem);

			var enabled = this.Options.ResourceOptions.LoadedResourcesPacks.Value;

			foreach (var resource in Alex.Resources.ResourcePackDirectory.EnumerateFiles())
			{
				try
				{
					if (resource.FullName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
					{
						if (Alex.Resources.TryLoadResourcePackInfo(
							    resource.FullName, out ResourcePackManifest[] packInfos))
						{
							bool isLoaded = enabled.Any(
								x => x.EndsWith(
									Path.GetFileName(resource.FullName), StringComparison.InvariantCultureIgnoreCase));

							foreach (var packInfo in packInfos)
							{
								var item = new ResourcePackEntry(packInfo, resource.FullName);
								item.SetLoaded(isLoaded);

								AddItem(item);
							}
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
			_newLoaded = new List<string>();

			base.OnShow();

			_originallyLoaded = Options.ResourceOptions.LoadedResourcesPacks.Value;
			_newLoaded.AddRange(_originallyLoaded);

			Reload();
		}
	}
}