using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Alex.Common;
using Alex.Common.Graphics;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Entities;
using Alex.Gamestates.Common;
using Alex.Graphics.Models.Entity;
using Alex.Gui;
using Alex.Gui.Elements.MainMenu;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Bedrock;
using Alex.ResourcePackLib.Json;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;
using RocketUI.Events;
using SixLabors.ImageSharp.Formats.Png;

namespace Alex.Gamestates.MainMenu
{
	public class SkinSelectionState : GuiMenuStateBase
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SkinSelectionState));

		private GuiPanoramaSkyBox _skyBox;
		private IStorageSystem Storage { get; }
		private Alex Alex { get; }

		private Button _cancelBtn, _selectBtn;

		protected SkinSelectionEntry[] Items => _items.ToArray();
		private List<SkinSelectionEntry> _items { get; } = new List<SkinSelectionEntry>();

		private SkinSelectionEntry _selectedItem;

		public SkinSelectionEntry SelectedItem
		{
			get => _selectedItem;
			set
			{
				if (_selectedItem == value) return;
				_selectedItem = value;

				OnSelectedItemChanged(value);
				//  SelectedItemChanged?.Invoke(this, _selectedItem);
			}
		}

		public SkinSelectionState(GuiPanoramaSkyBox skyBox, Alex alex)
		{
			Alex = alex;
			_skyBox = skyBox;
			Storage = GetService<IStorageSystem>();

			Title = "Select Skin";

			Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);

			Body.Orientation = Orientation.Vertical;
			//base.ListContainer.ChildAnchor = Alignment.MiddleCenter;
			// base.ListContainer.Orientation = Orientation.Horizontal;

			Footer.AddRow(
				row =>
				{
					//   row.ChildAnchor = Alignment.CenterX;
					row.AddChild(_selectBtn = new AlexButton("Select Skin", OnSkinSelect) { Enabled = false });

					row.AddChild(_cancelBtn = new AlexButton("Cancel", OnCancelButtonPressed) { });
				});

			Footer.AddRow(
				row =>
				{
					//row.ChildAnchor = Alignment.BottomCenter;
					row.AddChild(new AlexButton("Previous Page", PrevPage, false).ApplyModernStyle());
					row.AddChild(new AlexButton("Open SkinPack folder", OpenSkinPackFOlder, false).ApplyModernStyle());
					row.AddChild(new AlexButton("Next Page", NextPage, false).ApplyModernStyle());
				});

			Reload();
		}

		private void PrevPage()
		{
			LoadItems(Math.Max(0, _start - _count), _count);
		}

		private void NextPage()
		{
			LoadItems(_start + _count, _count);
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
			LoadItems(0, 39);
		}

		private StackContainer CreateRow()
		{
			return Body.AddRow(
				b =>
				{
					b.Orientation = Orientation.Horizontal;

					b.ChildRemoved += (sender, args) =>
					{
						if (!b.HasChildren)
							Body.RemoveChild(b);
					};
				});
		}

		public void ClearItems()
		{
			SelectedItem = null;

			foreach (var item in _items)
			{
				if (item.ParentElement is StackContainer stackContainer)
				{
					stackContainer.RemoveChild(item);
				}
				//Body.RemoveChild(item);
			}

			_items.Clear();
		}

		private int _start = 0;
		private int _count = 10;

		private void LoadItems(int start, int count)
		{
			ClearItems();

			var row = CreateRow();

			if (Alex.PlayerModel != null && Alex.PlayerTexture != null)
			{
				// Alex.UiTaskManager.Enqueue(
				//     () =>
				//    {
				// var texture = TextureUtils.BitmapToTexture2D(this, Alex.GraphicsDevice, Alex.PlayerTexture);

				SkinSelectionEntry selectionEntry = new SkinSelectionEntry(
					new LoadedSkin("Default", Alex.PlayerModel, Alex.PlayerTexture), OnDoubleClick);

				row.AddChild(selectionEntry);
				_items.Add(selectionEntry);
				//AddItem(entry);
				//    });
			}

			// LoadItems(0, 10);

			_start = start;
			_count = count;
			int index = 0;
			int processed = 1;

			foreach (var skinPack in Alex.Resources.SkinPacks)
			{
				if (index >= start + count)
					break;

				foreach (var module in skinPack.Modules.Where(x => x is SkinModule).Cast<SkinModule>())
				{
					if (index >= start + count)
						break;

					foreach (var skin in module.Info.Skins)
					{
						if (string.IsNullOrWhiteSpace(skin.Geometry))
							continue;

						index++;

						if (index < start)
							continue;

						if (ModelFactory.TryGetModel(skin.Geometry, out var model) && model != null)
						{
							if (module.TryGetBitmap(skin.Texture, out var bmp))
							{
								SkinSelectionEntry selectionEntry = new SkinSelectionEntry(
									new LoadedSkin(skin.LocalizationName, model, bmp), OnDoubleClick);

								//AddItem(entry);
								_items.Add(selectionEntry);
								row.AddChild(selectionEntry);
								processed++;

								if (processed % 5 == 0)
								{
									row = CreateRow();
								}
							}
						}

						if (index >= start + count)
							break;
						/*var renderer = EntityFactory.GetEntityRenderer(skin.Geometry);
						if (renderer == null)
						     continue;
						
					  
						
						Alex.UiTaskManager.Enqueue(
						    () =>
						    {
						        var texture = TextureUtils.BitmapToTexture2D(this, Alex.GraphicsDevice, skin.Texture);
						        SkinEntry entry = new SkinEntry(skin, texture, OnDoubleClick);
						        AddItem(entry);
						    });*/
					}
				}
			}
		}

		private void OnDoubleClick(SkinSelectionEntry profile)
		{
			Focus(profile);

			// profile.Skin.Model.Name = "geometry.alex.custom";
			// Storage.TryWriteString("skin.json", );

			using (MemoryStream ms = new MemoryStream())
			{
				profile.Skin.Texture.Save(ms, new PngEncoder());
				Storage.TryWriteBytes("skin.png", ms.ToArray());
			}

			Alex.PlayerModel = profile.Skin.Model;
			Alex.PlayerTexture = profile.Skin.Texture;

			Alex.PlayerModel.Description.TextureHeight = Alex.PlayerTexture.Height;
			Alex.PlayerModel.Description.TextureWidth = Alex.PlayerTexture.Width;
			Alex.PlayerModel.Description.Identifier = Alex.PlayerModel.Description.Identifier.Split(':')[0];

			Dictionary<string, object> mm = new Dictionary<string, object>();
			mm.Add("format_version", "1.12.0");
			mm.Add("minecraft:geometry", Alex.PlayerModel);
			//  GeometryModel mm       = new GeometryModel();
			//  mm.Geometry.Add(model.Description.Identifier, model);

			var geoData = Encoding.UTF8.GetBytes(MCJsonConvert.SerializeObject(mm));
			Storage.TryWriteBytes("skin.json", geoData);
			//  File.WriteAllBytes("currentSkin.json", geoData);

			OnCancelButtonPressed();
		}

		private void OnCancelButtonPressed()
		{
			Alex.GameStateManager.Back();
			// Alex.GameStateManager.SetActiveState<TitleState>("title");
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			// _skyBox.Update(gameTime);
		}

		/// <inheritdoc />
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			_skyBox.Draw(new RenderArgs()
			{
				GameTime = gameTime,
				GraphicsDevice = graphics.Context.GraphicsDevice,
				SpriteBatch = graphics.SpriteBatch
			});
			base.OnDraw(graphics, gameTime);
		}
		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
			GuiManager.FocusManager.FocusChanged += OnFocusChanged;
		}

		private void OnFocusChanged(object? sender, GuiFocusChangedEventArgs e)
		{
			if (e.FocusedElement == null || !(e.FocusedElement is SkinSelectionEntry listItem))
				return;

			if (!_items.Contains(listItem))
				return;

			SelectedItem = listItem;
			// SetSelectedItem(listItem);
		}

		protected override void OnLoad(IRenderArgs args)
		{
			base.OnLoad(args);
		}

		private SkinSelectionEntry _previous = null;

		protected void OnSelectedItemChanged(SkinSelectionEntry newItem)
		{
			if (_previous != null)
			{
				ClearFocus(_previous);
			}

			if (newItem != null)
			{
				SetButtonState(true);
			}
			else
			{
				SetButtonState(false);
			}
		}
	}
}