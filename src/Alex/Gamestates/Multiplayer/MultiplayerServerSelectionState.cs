using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Data.Servers;
using Alex.Common.Graphics;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.Gamestates.Login;
using Alex.Gui;
using Alex.Gui.Dialogs;
using Alex.Gui.Elements;
using Alex.Services;
using Alex.Utils;
using Alex.Worlds.Multiplayer.Bedrock;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NLog;
using RocketUI;

namespace Alex.Gamestates.Multiplayer
{
    public class MultiplayerServerSelectionState : ListSelectionStateBase<GuiServerListEntryElement>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiplayerServerSelectionState));

        private Button DirectConnectButton;
        private Button JoinServerButton;
        private Button AddServerButton;
        private Button EditServerButton;
        private Button DeleteServerButton;
        private Button RefreshButton;

        private readonly IListStorageProvider<SavedServerEntry> _listProvider;

        private GuiPanoramaSkyBox _skyBox;
        private CancellationTokenSource CancellationTokenSource { get; }
        private StackContainer _tabItemContainer { get; }

        private string _filterValue = "java";
        public MultiplayerServerSelectionState(GuiPanoramaSkyBox skyBox) : base()
        {
            _skyBox = skyBox;
            CancellationTokenSource = new CancellationTokenSource();

            _listProvider = GetService<IListStorageProvider<SavedServerEntry>>();

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

            stackedStack.AddChild(_tabItemContainer = new StackContainer()
            {
                Orientation = Orientation.Horizontal,
                ChildAnchor = Alignment.BottomLeft,
                BackgroundOverlay = new Color(Color.Black, 0.20f)
            });

            Header.AddChild(stackedStack);

            ActiveTabBtn = AddTabButton("Java", () =>
            {
                _filterValue = "java";
                this.Reload();
            });

            AddTabButton("Bedrock", () =>
            {
                _filterValue = "bedrock";
                this.Reload();
            });

            //_tabItemContainer.MinWidth = _tabItemContainer.Width = BodyMinWidth;
            /*_tabItemContainer.AddChild(new AlexButton("Java")
		    {
			    BackgroundOverlay = new Color(Color.Black, 0.25f),
			    Margin = Thickness.Zero
		    });
		    _tabItemContainer.AddChild(new AlexButton("Bedrock")
		    {
			    Margin = Thickness.Zero
		    });*/

            Footer.AddRow(row =>
            {
                row.AddChild(JoinServerButton = new AlexButton("Join Server",
                    OnJoinServerButtonPressed)
                {
                    TranslationKey = "selectServer.select",
                    Enabled = false
                });
                row.AddChild(DirectConnectButton = new AlexButton("Direct Connect",
                    () => Alex.GameStateManager.SetActiveState<MultiplayerConnectState>())
                {
                    TranslationKey = "selectServer.direct",
                    Enabled = false
                });
                row.AddChild(AddServerButton = new AlexButton("Add Server",
                    OnAddItemButtonPressed)
                {
                    TranslationKey = "selectServer.add"
                });
            });
            Footer.AddRow(row =>
            {
                row.AddChild(EditServerButton = new AlexButton("Edit", OnEditItemButtonPressed)
                {
                    TranslationKey = "selectServer.edit",
                    Enabled = false
                });
                row.AddChild(DeleteServerButton = new AlexButton("Delete", OnDeleteItemButtonPressed)
                {
                    TranslationKey = "selectServer.delete",
                    Enabled = false
                });
                row.AddChild(RefreshButton = new AlexButton("Refresh", OnRefreshButtonPressed)
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
                    oldValue.DefaultColor = (Color)TextColor.DarkGray;

                    oldValue.Enabled = false;
                    oldValue.Enabled = true;
                }

                value.BackgroundOverlay = new Color(Color.Black, 0.25f);
                value.DefaultColor = (Color)TextColor.White;
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
                DefaultColor = (Color)TextColor.DarkGray
            };

            button.Action = () =>
            {
                ActiveTabBtn = button;
                action?.Invoke();
            };

            _tabItemContainer.AddChild(button);

            return button;
        }

        private bool _isShown = false;
        protected override void OnShow()
        {
            base.OnShow();

            if (_isShown)
                return;

            _isShown = true;

            //  var queryProvider = GetService<IServerQueryProvider>();

            _listProvider.Load();

            Reload();
        }

        private CancellationTokenSource _cancellationTokenSource;

        private void Reload()
        {

            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                var source = _cancellationTokenSource;
                var token = source.Token;
                source.CancelAfter(TimeSpan.FromSeconds(10));

                ClearItems();

                Task previousTask = null;

                if (Alex.ServerTypeManager.TryGet(_filterValue, out var serverType))
                {
                    void setPrevious(Task task)
                    {
                        if (previousTask != null && !previousTask.IsCompleted)
                        {
                            previousTask = previousTask.ContinueWith(async r => await task, token);
                        }
                        else
                        {
                            previousTask = task;
                        }
                    }

                    foreach (var entry in serverType.SponsoredServers)
                    {
                        var item = new GuiServerListEntryElement(serverType, entry)
                        {
                            CanDelete = false,
                            SaveEntry = false
                        };

                        AddItem(item);

                        setPrevious(item.PingAsync(false, token));
                    }

                    serverType.QueryProvider.StartLanDiscovery(
                        token, async r =>
                        {
                            if (r.QueryResponse.Success)
                            {
                                GuiServerListEntryElement entry = new GuiServerListEntryElement(
                                    serverType,
                                    new SavedServerEntry()
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
                                        //entry.ServerName = $"[LAN] {r.QueryResponse.Status.Query.Description.Text}";

                                        AddItem(entry);

                                await entry.PingAsync(false, token).ConfigureAwait(false);
                            }
                        }).ConfigureAwait(false);

                    foreach (var entry in _listProvider.Data.Where(x => x.ServerType.Equals(_filterValue))
                       .ToArray())
                    {
                        var element = new GuiServerListEntryElement(serverType, entry);
                        AddItem(element);

                        setPrevious(element.PingAsync(false, token));
                    }
                }
            }
            finally { }
        }

        protected override void OnSelectedItemChanged(GuiServerListEntryElement newItem)
        {
            if (newItem != null)
            {
                JoinServerButton.Enabled = true;
                EditServerButton.Enabled = newItem.SaveEntry;
                DeleteServerButton.Enabled = newItem.CanDelete;
            }
            else
            {
                JoinServerButton.Enabled = false;
                EditServerButton.Enabled = false;
                DeleteServerButton.Enabled = false;
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
                    Log.Info($"Removing item: {toDelete.SavedServerEntry.Name}");
                    RemoveItem(toDelete);

                    if (_listProvider.RemoveEntry(toDelete.SavedServerEntry))
                    {
                        //_listProvider.Save(_listProvider.Data);

                        Log.Info($"Reloading: {toDelete.SavedServerEntry.Name}");
                        Reload();
                    }
                    else
                    {
                        Log.Warn($"Failed to remove item.");
                    }
                }
            }));
        }

        private async void OnJoinServerButtonPressed()
        {
            CancellationTokenSource?.Cancel();
            var overlay = new LoadingOverlay();
            Alex.GuiManager.AddScreen(overlay);

            try
            {
                var selectedItem = SelectedItem;
                var entry = selectedItem.SavedServerEntry;

                var profileManager = GetService<ProfileManager>();
                if (Alex.ServerTypeManager.TryGet(entry.ServerType, out var typeImplementation))
                {
                    var profiles = profileManager.GetProfiles(entry.ServerType);
                    PlayerProfile currentProfile = null;

                    if (profiles.Length == 1)
                    {
                        currentProfile = profiles[0];
                    }

                    void Connect(PlayerProfile profile)
                    {
                        var target = selectedItem.ConnectionEndpoint;
                        if (target == null)
                        {
                            Alex.GuiManager.RemoveScreen(overlay);
                            overlay = null;
                            return;
                        }

                        Alex.ConnectToServer(
                            typeImplementation, new ServerConnectionDetails(target, entry.Host),
                            profile);

                        Alex.GuiManager.RemoveScreen(overlay);
                        overlay = null;
                    }

                    if (currentProfile == null || !await typeImplementation.VerifyAuthentication(currentProfile))
                    {
                        await typeImplementation.Authenticate(
                            _skyBox, currentProfile, () =>
                            {
                                Connect(profileManager.CurrentProfile);
                            });
                    }
                    else
                    {
                        Connect(currentProfile);
                    }
                }
            }
            finally
            {
                if (overlay != null)
                    Alex.GuiManager.RemoveScreen(overlay);
            }
        }

        private void OnCancelButtonPressed()
        {
            Alex.GameStateManager.Back();
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

            _listProvider.Save(_listProvider.Data);
        }

        private void AddEditServerCallbackAction(MultiplayerAddEditServerState.AddOrEditCallback obj)
        {
            if (obj == null) return; //Cancelled.

            if (!obj.IsNew)
            {
                var entry = Items.FirstOrDefault(
                    x => x.SavedServerEntry.InternalIdentifier.Equals(obj.Entry.InternalIdentifier));

                if (entry != null)
                {
                    _listProvider.RemoveEntry(entry.SavedServerEntry);
                    entry.SavedServerEntry = obj.Entry;
                    _listProvider.AddEntry(entry.SavedServerEntry);

                    entry.PingAsync(false, CancellationToken.None);
                }
            }
            else
            {
                _listProvider.AddEntry(obj.Entry);
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

            CancellationTokenSource.Cancel();

            SaveAll();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
        }
    }
}
