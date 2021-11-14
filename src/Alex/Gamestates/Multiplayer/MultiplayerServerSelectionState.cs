using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Data.Servers;
using Alex.Common.Graphics;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Utils;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.Gamestates.Multiplayer
{
    public class MultiplayerServerSelectionState : ListSelectionStateBase<GuiServerListEntryElement>
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiplayerServerSelectionState));
	    
	    private Button _directConnectButton;
	    private Button _joinServerButton;
	    private Button _addServerButton;
	    private Button _editServerButton;
	    private Button _deleteServerButton;
	    private Button _refreshButton;

	   // private readonly IListStorageProvider<SavedServerEntry> _listProvider;

	    private readonly GuiPanoramaSkyBox       _skyBox;
	    private CancellationTokenSource CancellationTokenSource { get; }
	    private StackContainer TabItemContainer { get; }
	    
	    private ServerTypeImplementation _serverType;
		public MultiplayerServerSelectionState() : base()
		{
			var alex = GetService<Alex>();
			_skyBox = GetService<GuiPanoramaSkyBox>();
			CancellationTokenSource = new CancellationTokenSource();
			
			//_listProvider = GetService<IListStorageProvider<SavedServerEntry>>();

		    Title = "Multiplayer";
		    TitleTranslationKey = "multiplayer.title";

		    Header.Height = 44;
		    Header.Padding = new Thickness(3, 3, 3, 0);
		    Header.Margin = new Thickness(3, 3, 3, 0);

		    StackContainer stackedStack = new StackContainer()
		    {
			    Orientation = Orientation.Horizontal, 
			    ChildAnchor = Alignment.BottomLeft,
			    MinWidth = BodyMinWidth,
			    Width = BodyMinWidth,
			    
		    };
		    
		    stackedStack.AddChild(TabItemContainer = new StackContainer()
		    {
			    Orientation = Orientation.Horizontal,
			    ChildAnchor = Alignment.BottomLeft,
			    BackgroundOverlay = new Color(Color.Black, 0.20f)
		    });
		    
		    Header.AddChild(stackedStack);

		    foreach (var serverType in alex.ServerTypeManager.GetAll())
		    {
			    var button = AddTabButton(serverType.DisplayName, () =>
			    {
				    _serverType = serverType;
				    this.Reload();
			    });

			    if (ActiveTabBtn == null)
			    {
				    ActiveTabBtn = button;
				    _serverType = serverType;
			    }
		    }

		    Footer.AddRow(row =>
		    {
			    row.AddChild(_joinServerButton = new AlexButton("Join Server",
				    OnJoinServerButtonPressed)
			    {
				    TranslationKey = "selectServer.select",
				    Enabled = false
			    });
			    row.AddChild(_directConnectButton = new AlexButton("Direct Connect",
				    () => Alex.GameStateManager.SetActiveState<MultiplayerConnectState>())
			    {
				    TranslationKey = "selectServer.direct",
				    Enabled = false
			    });
			    row.AddChild(_addServerButton = new AlexButton("Add Server",
				    OnAddItemButtonPressed)
			    {
				    TranslationKey = "selectServer.add"
			    });
		    });
		    Footer.AddRow(row =>
		    {
			    row.AddChild(_editServerButton = new AlexButton("Edit", OnEditItemButtonPressed)
			    {
				    TranslationKey = "selectServer.edit",
				    Enabled = false
			    });
			    row.AddChild(_deleteServerButton = new AlexButton("Delete", OnDeleteItemButtonPressed)
			    {
				    TranslationKey = "selectServer.delete",
				    Enabled = false
			    });
			    row.AddChild(_refreshButton = new AlexButton("Refresh", OnRefreshButtonPressed)
			    {
				    TranslationKey = "selectServer.refresh"
			    });
			    row.AddChild(new GuiBackButton()
			    {
				    TranslationKey = "gui.cancel"
			    });
		    });

		    Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);
		    
		    Body.Margin = new Thickness(0, Header.Height, 0, Footer.Height);
		}

		private Button _activeTabBtn;
		private Button ActiveTabBtn
		{
			get => _activeTabBtn;
			set
			{
				var oldValue = _activeTabBtn;

				if (oldValue != null)
				{
					oldValue.BackgroundOverlay = Color.Transparent;
					oldValue.DefaultColor = (Color) TextColor.DarkGray;
					
					oldValue.Enabled = false;
					oldValue.Enabled = true;
				}

				value.BackgroundOverlay = new Color(Color.Black, 0.25f);
				value.DefaultColor = (Color) TextColor.White;
				_activeTabBtn = value;
			}
		}

		private Button AddTabButton(string text, Action action)
		{
			Button button = new AlexButton(
				text)
			{
				Margin = Thickness.Zero,
				BackgroundOverlay = Color.Transparent,
				DefaultColor = (Color) TextColor.DarkGray
			};

			button.Action = () =>
			{
				ActiveTabBtn = button;
				action?.Invoke();
			};
			
			TabItemContainer.AddChild(button);

			return button;
		}

		private bool _isShown = false;
	    protected override void OnShow()
	    {
		    base.OnShow();

		    if (_isShown)
			    return;

		    _isShown = true;

		    Reload();
	    }

	    private CancellationTokenSource _cancellationTokenSource;

	    private void Reload()
	    {
		    try
		    {
			    _cancellationTokenSource?.Cancel();
			    _cancellationTokenSource = new CancellationTokenSource();
			    
			    ClearItems();

			    var serverType = _serverType;
			    if (serverType == null) 
				    return;
			    
			    serverType.StorageProvider.Load();

			    foreach (var entry in serverType.SponsoredServers)
			    {
				    var item = CreateItem(serverType, entry);
				    item.CanDelete = false;
				    item.SaveEntry = false;
			    }

			    foreach (var entry in serverType.StorageProvider.Data.ToArray())
			    {
				    CreateItem(serverType, entry);
			    }
			    
			    serverType.QueryProvider.StartLanDiscovery(
				    _cancellationTokenSource.Token, LanDiscoveryCallback).ConfigureAwait(false);
		    }
		    finally { }
	    }

	    private GuiServerListEntryElement CreateItem(ServerTypeImplementation serverType, SavedServerEntry serverEntry)
	    {
		    var item = new GuiServerListEntryElement(serverType, serverEntry);
		    AddItem(item);

		    return item;
	    }

	    /// <inheritdoc />
	    protected override void OnItemDoubleClick(GuiServerListEntryElement item)
	    {
		    base.OnItemDoubleClick(item);
		    
		    if (SelectedItem != item)
			    return;
		    
		    JoinServer(item);
	    }

	    private Task LanDiscoveryCallback(LanDiscoveryResult r)
	    {
		    var serverType = _serverType;

		    if (serverType == null)
			    return Task.CompletedTask;
		    
		    if (r.QueryResponse.Success)
		    {
			    var entry = CreateItem(
				    serverType, new SavedServerEntry()
				    {
					    ServerType = serverType.Id,
					    Host = r.QueryResponse.Status.Address,
					    Port = r.QueryResponse.Status.Port,
					    Name = $"[LAN] {r.EndPoint}",
					    InternalIdentifier = Guid.NewGuid()
				    });

			    entry.SaveEntry = false;
			    entry.CanDelete = false;

			    entry.ConnectionEndpoint = r.EndPoint;

			    AddItem(entry);
		    }

		    return Task.CompletedTask;
	    }
	    
	    private Task _previousTask = null;

	    /// <inheritdoc />
	    protected override void OnAddItem(GuiServerListEntryElement item)
	    {
		    base.OnAddItem(item);

		    var prevTask = _previousTask;

		    if (prevTask == null || prevTask.IsCompleted)
		    {
			    _previousTask = item.PingAsync(false, _cancellationTokenSource.Token);
		    }
		    else
		    {
			    _previousTask = prevTask.ContinueWith(_ => item.PingAsync(false, _cancellationTokenSource.Token));
		    }
		    
		    //item?.PingAsync(false, _cancellationTokenSource.Token);
	    }

	    protected override void OnSelectedItemChanged(GuiServerListEntryElement newItem)
	    {
		    if (newItem != null)
		    {
			    _joinServerButton.Enabled = true;
			    _editServerButton.Enabled = newItem.SaveEntry;
			    _deleteServerButton.Enabled = newItem.CanDelete;
		    }
		    else
		    {
			    _joinServerButton.Enabled = false;
			    _editServerButton.Enabled   = false;
			    _deleteServerButton.Enabled = false;
		    }
	    }
		
	    public void OnAddItemButtonPressed()
	    {
		    Alex.GameStateManager.SetActiveState(new MultiplayerAddEditServerState(AddEditServerCallbackAction, _skyBox));
	    }

	    private void OnEditItemButtonPressed()
	    {
		    Alex.GameStateManager.SetActiveState(new MultiplayerAddEditServerState(SelectedItem.SavedServerEntry, AddEditServerCallbackAction, _skyBox));
	    }
		
	    private void OnDeleteItemButtonPressed()
	    {
		    var toDelete = SelectedItem;
		    Alex.GameStateManager.SetActiveState(new GuiConfirmState(new GuiConfirmState.GuiConfirmStateOptions()
		    {
				MessageTranslationKey = "selectServer.deleteQuestion",
				ConfirmTranslationKey = "selectServer.deleteButton"
		    }, confirm =>
		    {
			    if (confirm)
			    {
				    RemoveItem(toDelete);

				    var serverType = _serverType;
				    if (serverType != null && serverType.StorageProvider.RemoveEntry(toDelete.SavedServerEntry))
				    {
					    Reload();
				    }
				    else
				    {
					    Log.Warn($"Failed to remove item.");
				    }
			    }
		    }));
	    }

	    private async void JoinServer(GuiServerListEntryElement item)
	    {
		    if (item == null)
			    return;
		    
		    var serverType = _serverType;
		    if (serverType == null)
			    return;

		    CancellationTokenSource?.Cancel();
		    var overlay = new LoadingOverlay();
		    Alex.GuiManager.AddScreen(overlay);

		    try
		    {
			    var entry = item.SavedServerEntry;

			    bool handledCallback = false;
			    await serverType.Authenticate(
				    _skyBox, (profile) =>
				    {
					    if (handledCallback)
						    return;

					    handledCallback = true;
					    
					    if (overlay != null)
					    {
						    Alex.GuiManager.RemoveScreen(overlay);
						    overlay = null;
					    }

					    if (!profile.Authenticated)
					    {
						    return;
					    }

					    var target = item.ConnectionEndpoint;

					    if (target == null)
						    return;

					    Alex.ConnectToServer(
						    serverType, new ServerConnectionDetails(target, entry.Host), profile);
				    });
		    }
		    finally
		    {
			    if (overlay != null)
				    Alex.GuiManager.RemoveScreen(overlay);
		    }
	    }
	    
	    private void OnJoinServerButtonPressed()
	    {
		    JoinServer(SelectedItem);
	    }

	    private void OnRefreshButtonPressed()
	    {
		    Reload();
	    }

	    private void SaveAll()
	    {
		    foreach (var item in Items.ToArray())
		    {
			    if (!item.SaveEntry)
			    {
				    RemoveItem(item);
			    }
		    }
		    
		    var serverType = _serverType;

		    if (serverType == null)
			    return;
		    
		    serverType.StorageProvider?.Save();
	    }

	    private void AddEditServerCallbackAction(MultiplayerAddEditServerState.AddOrEditCallback obj)
	    {
		    if (obj == null) return; //Cancelled.
		    
		    var storageProvider = _serverType?.StorageProvider;
		    
		    if (!obj.IsNew)
		    {
			    var entry = Items.FirstOrDefault(
				    x => x.SavedServerEntry.InternalIdentifier.Equals(obj.Entry.InternalIdentifier));

			    if (entry != null)
			    {
				    storageProvider?.RemoveEntry(entry.SavedServerEntry);
				    entry.SavedServerEntry = obj.Entry;
				    storageProvider?.AddEntry(entry.SavedServerEntry);

				    entry.PingAsync(false, CancellationToken.None);
			    }
		    }
		    else
		    {
			    storageProvider?.AddEntry(obj.Entry);
		    }

		    SaveAll();

		    Reload();
	    }

	    protected override void OnUpdate(GameTime gameTime)
	    {
			_skyBox.Update(gameTime);
		    base.OnUpdate(gameTime);
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

	    protected override void OnHide()
	    {
		    base.OnHide();
		    
		    _cancellationTokenSource?.Cancel();
		    _cancellationTokenSource = null;
		    
		    CancellationTokenSource?.Cancel();
		    
			SaveAll();
	    }
    }
}
