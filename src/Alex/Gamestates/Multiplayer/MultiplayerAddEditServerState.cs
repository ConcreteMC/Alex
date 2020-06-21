using System;
using Alex.API.Data.Servers;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NLog;
using RocketUI;

namespace Alex.Gamestates.Multiplayer
{
	public class MultiplayerAddEditServerState : GuiCallbackStateBase<MultiplayerAddEditServerState.AddOrEditCallback>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiplayerConnectState));

		#region Gui Elements

		private readonly GuiTextInput    _nameInput;
		private readonly GuiTextInput    _hostnameInput;
		private readonly GuiTextInput    _portInput;
		private readonly GuiTextElement  _errorMessage;
		private readonly GuiButton       _saveButton;
	//	private readonly GuiToggleButton _javaEditionButton;
		//private readonly GuiToggleButton _bedrockEditionButton;
		private readonly GuiTextElement  _serverTypeLabel;
		private readonly GuiButtonGroup  _serverTypeGroup;
		
		#endregion

		private readonly SavedServerEntry                       _entry = null;
	//	private readonly IListStorageProvider<SavedServerEntry> _savedServersStorage;
		private readonly GuiPanoramaSkyBox                      _skyBox;
		private readonly ServerTypeManager _serverTypeManager;
		
		public MultiplayerAddEditServerState(Action<AddOrEditCallback> callbackAction, GuiPanoramaSkyBox skyBox) :
			this("java", null, null, callbackAction, skyBox)
		{
		}

		private ServerTypeImplementation _selectedImplementation = null;
		public MultiplayerAddEditServerState(string serverType, string name, string address,
											 Action<AddOrEditCallback> callbackAction,
											 GuiPanoramaSkyBox        skyBox) :
			base(callbackAction)
		{
			_serverTypeManager = GetService<ServerTypeManager>();
			_skyBox              = skyBox;

			Title = "Add Server";
			TitleTranslationKey = "addServer.title";

			base.HeaderTitle.Anchor    = Alignment.MiddleCenter;
			base.HeaderTitle.FontStyle = FontStyle.Bold | FontStyle.DropShadow;
			Body.BackgroundOverlay     = new Color(Color.Black, 0.5f);

			Body.ChildAnchor = Alignment.MiddleCenter;

			var usernameRow = AddGuiRow(new GuiTextElement()
			{
				Text   = "Server Name:",
				TranslationKey = "addServer.enterName",
				Margin = new Thickness(0, 0, 5, 0)
			}, _nameInput = new GuiTextInput()
			{
				TabIndex = 1,

				Width = 200,

				PlaceHolder = "Name of the server",
				Margin      = new Thickness(23, 5, 5, 5),
			});
			usernameRow.ChildAnchor = Alignment.MiddleCenter;
			usernameRow.Orientation = Orientation.Horizontal;

			var hostnameRow = AddGuiRow(new GuiTextElement()
			{
				Text   = "Server Address:",
				TranslationKey = "addServer.enterIp",
				Margin = new Thickness(0, 0, 5, 0)
			}, _hostnameInput = new GuiTextInput()
			{
				TabIndex = 2,

				Width = 200,

				PlaceHolder = "Hostname or IP",
				Margin      = new Thickness(5),
			});
			hostnameRow.ChildAnchor = Alignment.MiddleCenter;
			hostnameRow.Orientation = Orientation.Horizontal;

			var typeLabelRow = AddGuiRow(_serverTypeLabel = new GuiTextElement()
			{
				Text   = "Server Type:",
				Margin = new Thickness(0, 0, 5, 0)
			});
			typeLabelRow.ChildAnchor = Alignment.MiddleCenter;
			typeLabelRow.Orientation = Orientation.Horizontal;

			AddGuiRow(_serverTypeGroup = new GuiButtonGroup()
			{
				Orientation = Orientation.Horizontal,
				ChildAnchor = Alignment.MiddleCenter
			});

			int tabIndex = 3;
			foreach (var type in _serverTypeManager.GetAll())
			{
				if (_selectedImplementation == null)
					_selectedImplementation = type;

				GuiToggleButton element;
				_serverTypeGroup.AddChild(
					element = new GuiToggleButton(type.DisplayName)
					{
						Margin = new Thickness(5),
						Modern = true,
						Width = 50,
						Checked = serverType == type.Id,
						CheckedOutlineThickness = new Thickness(1),
						DisplayFormat = new ValueFormatter<bool>((val) => $"{type.DisplayName} {(val ? "[Active]" : "")}"),
						TabIndex = tabIndex++
					});

				element.ValueChanged += (sender, value) =>
				{
					if (value)
					{
						_selectedImplementation = type;
					}
				};
			}

			var buttonRow = AddGuiRow(_saveButton = new GuiButton(OnSaveButtonPressed)
			{
				AccessKey = Keys.Enter,

				TranslationKey = "addServer.add",
				Margin         = new Thickness(5),
				Modern         = false,
				Width          = 100,
				TabIndex = 5
			}, new GuiButton(OnCancelButtonPressed)
			{
				AccessKey = Keys.Escape,

				TranslationKey = "gui.cancel",
				Margin         = new Thickness(5),
				Modern         = false,
				Width          = 100,
				TabIndex = 6
			});
			buttonRow.ChildAnchor = Alignment.MiddleCenter;


			AddGuiElement(_errorMessage = new GuiTextElement()
			{
				TextColor = TextColor.Red
			});

			if (!string.IsNullOrWhiteSpace(name))
			{
				_nameInput.Value = name;
			}

			if (!string.IsNullOrWhiteSpace(address))
			{
				_hostnameInput.Value = address;
			}

			if (_entry != null)
			{
				//EnableButtonsFor(_entry.ServerType);
			}

			Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);
		}

		public MultiplayerAddEditServerState(SavedServerEntry  entry, Action<AddOrEditCallback> callbackAction,
											 GuiPanoramaSkyBox skyBox) : this(entry.ServerType, entry.Name, entry.Host + ":" + entry.Port,
																			  callbackAction, skyBox)
		{
			_entry = entry;
		}

		private void OnSaveButtonPressed()
		{
			try
			{
				var name    = _nameInput.Value;
				var address = _hostnameInput.Value;

				ushort port = (ushort) (_selectedImplementation.DefaultPort);

				var split    = address.Split(':', StringSplitOptions.RemoveEmptyEntries);
				var hostname = split[0];

				if (split.Length == 2)
				{
					if (ushort.TryParse(split[1], out port))
					{
						SaveServer(name, hostname, port);
					}
					else
					{
						SetErrorMessage("Invalid Server Address!");
					}
				}
				else if (split.Length == 1)
				{
					SaveServer(name, hostname, port);
				}
				else
				{
					SetErrorMessage("Invalid Server Address!");
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Error: {ex.ToString()}");
				SetErrorMessage(ex.Message);
			}
		}

		private void OnCancelButtonPressed()
		{
			InvokeCallback(null);
		}

		private void SaveServer(string name, string hostname, ushort port)
		{
			var entry = new SavedServerEntry()
			{
				Name       = name,
				Host       = hostname,
				Port       = port,
				ServerType = _selectedImplementation.Id,
				CachedIcon = _entry?.CachedIcon ?? null,
				ListIndex  = _entry?.ListIndex ?? -1
			};

			if (_entry != null)
			{
				entry.InternalIdentifier = _entry.InternalIdentifier;
			}
			
		/*	if (_entry != null)
			{
				_savedServersStorage.RemoveEntry(_entry);
			}

			_savedServersStorage.AddEntry(entry);*/

			InvokeCallback(new AddOrEditCallback(entry, _entry == null));
		}

		private void SetErrorMessage(string error)
		{
			_errorMessage.Text = error;
		}

		protected override void OnDraw(IRenderArgs args)
		{
			base.OnDraw(args);
			_skyBox.Draw(args);
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			_skyBox.Update(gameTime);
		}

		public class AddOrEditCallback
		{
			public SavedServerEntry Entry { get; }
			public bool IsNew { get; }

			public AddOrEditCallback(SavedServerEntry entry, bool isNew)
			{
				Entry = entry;
				IsNew = isNew;
			}
		}
	}
}