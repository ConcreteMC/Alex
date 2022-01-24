using System;
using System.Net;
using System.Threading;
using Alex.Common.Graphics;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Gamestates.Common;
using Alex.Gamestates.Multiplayer;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Utils;
using Alex.Worlds.Multiplayer;
using Alex.Worlds.Multiplayer.Bedrock;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Utils;
using MiNET.Worlds;
using RocketUI;

namespace Alex.Gamestates.Singleplayer;

public class WorldSelectionState : ListSelectionStateBase<WorldListEntry>
{
	private readonly GuiPanoramaSkyBox _skyBox;
	private readonly IStorageSystem _storageSystem;

	private AlexButton _playButton;
	private AlexButton _createButton;
	private AlexButton _deleteButton;
	private AlexButton _editButton;
	private AlexButton _recreateButton;

	public WorldSelectionState(GuiPanoramaSkyBox skyBox, IStorageSystem storageSystem)
	{
		TitleTranslationKey = "selectWorld.title";

		_skyBox = skyBox;
		_storageSystem = storageSystem.Open("storage", "worlds");

		Header.Padding = new Thickness(3, 3, 3, 0);
		Header.Margin = new Thickness(3, 3, 3, 0);

		Footer.AddRow(
			row =>
			{
				row.AddChild(
					_playButton =
						new AlexButton("Play", OnPlayButtonPressed)
						{
							TranslationKey = "selectWorld.select", Enabled = false
						});

				row.AddChild(
					_createButton = new AlexButton("New World", CreateWorldButtonPressed)
					{
						TranslationKey = "selectWorld.create", Enabled = false
					});
			});

		Footer.AddRow(
			row =>
			{
				row.AddChild(
					_editButton =
						new AlexButton("Edit", OnEditButtonPressed)
						{
							TranslationKey = "selectWorld.edit", Enabled = false
						});

				row.AddChild(
					_deleteButton =
						new AlexButton("Delete", OnDeletePressed)
						{
							TranslationKey = "selectWorld.delete", Enabled = false
						});

				row.AddChild(
					_recreateButton =
						new AlexButton("Re-Create", OnRecreatePressed) { TranslationKey = "selectWorld.recreate" });

				row.AddChild(new GuiBackButton() { TranslationKey = "gui.cancel" });
			});

		Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);

		Body.Margin = new Thickness(0, Header.Height, 0, Footer.Height);
		SetButtonState(false);
	}

	/// <inheritdoc />
	protected override void OnSelectedItemChanged(WorldListEntry newItem)
	{
		base.OnSelectedItemChanged(newItem);

		SetButtonState(newItem != null);
	}

	private void SetButtonState(bool hasSelectedWorld)
	{
		_playButton.Enabled = hasSelectedWorld && (SelectedItem?.WorldInfo.IsCompatible ?? false);
		_editButton.Enabled = false;
		_deleteButton.Enabled = hasSelectedWorld;
		_recreateButton.Enabled = false;
	}

	/// <inheritdoc />
	protected override void OnLoad(IRenderArgs args)
	{
		base.OnLoad(args);

		foreach (var directory in _storageSystem.EnumerateDirectories())
		{
			WorldInfo worldInfo = new WorldInfo(_storageSystem.Open(directory), directory);

			if (worldInfo.Initiate())
			{
				AddItem(new WorldListEntry(worldInfo));
			}
		}
	}

	private void OnRecreatePressed()
	{
		throw new System.NotImplementedException();
	}

	private void OnDeletePressed()
	{
		var selectedItem = SelectedItem;

		if (selectedItem == null)
			return;

		Alex.GameStateManager.SetActiveState(
			new GuiConfirmState(
				new GuiConfirmState.GuiConfirmStateOptions()
				{
					WarningTranslationKey = "selectWorld.deleteWarning",
					WarningParameters = new[] { selectedItem.WorldInfo.Name },
					MessageTranslationKey = "selectWorld.deleteQuestion",
					ConfirmTranslationKey = "selectWorld.deleteButton",
					CancelTranslationKey = "gui.cancel"
				}, confirm =>
				{
					if (confirm)
					{
						_storageSystem.Delete(selectedItem.WorldInfo.Name);
						RemoveItem(selectedItem);
					}
				}), true, false);
	}

	private void OnEditButtonPressed()
	{
		throw new System.NotImplementedException();
	}

	private void CreateWorldButtonPressed()
	{
		throw new System.NotImplementedException();
	}

	private void OnPlayButtonPressed()
	{
		var selected = SelectedItem?.WorldInfo;

		if (selected == null)
			return;

		if (_storageSystem.TryGetDirectory(selected.Name, out var directoryInfo))
		{
			AlexConfigProvider.Instance.Set("PCWorldFolder", directoryInfo.FullName);

			ThreadPool.QueueUserWorkItem(
				o =>
				{
					var overlay = Alex.GuiManager.CreateDialog<GenericLoadingDialog>();
					overlay.Show();

					overlay.Text = "Loading...";

					try
					{
						IWorldProvider worldProvider = null;

						string levelId = Dimension.Overworld.ToString();

						switch (selected.Type)
						{
							case WorldType.Anvil:
								var awp = new AnvilWorldProvider(directoryInfo.FullName);
								awp.Dimension = Dimension.Overworld;
								awp.Initialize();

								levelId = awp.Dimension.ToString();
								worldProvider = awp;

								break;

							case WorldType.LevelDB:
								var ldb = new LevelDbProvider(directoryInfo.FullName);
								ldb.Dimension = Dimension.Overworld;
								ldb.Initialize();

								levelId = ldb.Dimension.ToString();
								worldProvider = ldb;

								break;
						}

						if (worldProvider == null)
							return;

						AlexConfigProvider.Instance.Set("max-players", "1");
						MiNetServer ms = new MiNetServer();
						var levelManager = new LevelManager();

						var level = new Level(levelManager, levelId, worldProvider, levelManager.EntityManager)
						{
							EnableBlockTicking = true,
							EnableChunkTicking = true,
							SaveInterval = 300,
							UnloadInterval = 0,
							DrowningDamage = true,
							CommandblockOutput = true,
							DoTiledrops = true,
							DoMobloot = true,
							KeepInventory = false,
							DoDaylightcycle = true,
							DoMobspawning = true,
							DoEntitydrops = true,
							DoFiretick = true,
							DoWeathercycle = true,
							Pvp = true,
							Falldamage = true,
							Firedamage = true,
							Mobgriefing = true,
							ShowCoordinates = true,
							NaturalRegeneration = true,
							TntExplodes = true,
							SendCommandfeedback = true,
							RandomTickSpeed = 3,
						};

						level.Dimension = Dimension.Overworld;
						level.Initialize();
						levelManager.Levels.Add(level);

						ms.LevelManager = levelManager;

						var provider = new SingleplayerBedrockWorldProvider(
							Alex, ms,
							new PlayerProfile("a019fb4b-1de4-4ddb-96a8-06f7a27f7b48", "Steve", "Steve", "", "") { });

						provider.Init(out var networkProvider);

						Alex.LoadWorld(provider, networkProvider, false);
					}
					finally
					{
						overlay.Close();
					}
				});
		}
	}
}