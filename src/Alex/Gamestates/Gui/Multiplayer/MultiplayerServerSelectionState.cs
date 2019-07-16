using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Data.Servers;
using Alex.API.Graphics;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Icons;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.GameStates.Gui.Common;
using Alex.Gamestates.Login;
using Alex.Gui;
using Alex.Networking.Java;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using RocketUI;

namespace Alex.GameStates.Gui.Multiplayer
{
    public class MultiplayerServerSelectionState : ListSelectionStateBase<GuiServerListEntryElement>
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiplayerServerSelectionState));
	    
	    private GuiButton DirectConnectButton;
	    private GuiButton JoinServerButton;
	    private GuiButton AddServerButton;
	    private GuiButton EditServerButton;
	    private GuiButton DeleteServerButton;

	    private readonly IListStorageProvider<SavedServerEntry> _listProvider;

	    private GuiPanoramaSkyBox _skyBox;
		public MultiplayerServerSelectionState(GuiPanoramaSkyBox skyBox) : base()
		{
			_skyBox = skyBox;

			_listProvider = Alex.Services.GetService<IListStorageProvider<SavedServerEntry>>();

		    Title = "Multiplayer";

			Footer.AddRow(row =>
		    {

			    row.AddChild(JoinServerButton = new GuiButton("Join Server",
				    OnJoinServerButtonPressed)
			    {
				    TranslationKey = "selectServer.select",
				    Enabled = false
			    });
			    row.AddChild(DirectConnectButton = new GuiButton("Direct Connect",
				    () => Alex.GameStateManager.SetActiveState<MultiplayerConnectState>())
			    {
				    TranslationKey = "selectServer.direct"
			    });
			    row.AddChild(AddServerButton = new GuiButton("Add Server",
				    OnAddItemButtonPressed)
			    {
				    TranslationKey = "selectServer.add"
			    });
		    });
		    Footer.AddRow(row =>
		    {
			    row.AddChild(EditServerButton = new GuiButton("Edit", OnEditItemButtonPressed)
			    {
				    TranslationKey = "selectServer.edit",
				    Enabled = false
			    });
			    row.AddChild(DeleteServerButton = new GuiButton("Delete", OnDeleteItemButtonPressed)
			    {
				    TranslationKey = "selectServer.delete",
				    Enabled = false
			    });
			    row.AddChild(new GuiButton("Refresh", OnRefreshButtonPressed)
			    {
				    TranslationKey = "selectServer.refresh"
			    });
			    row.AddChild(new GuiButton("Cancel", OnCancelButtonPressed)
			    {
				    TranslationKey = "gui.cancel"
			    });
		    });

		    Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);
		}

	    protected override void OnShow()
	    {
		    base.OnShow();
		    Load(true);
		}

	    protected override void OnSelectedItemChanged(GuiServerListEntryElement newItem)
	    {
		    if (newItem != null)
		    {
			    JoinServerButton.Enabled = true;
			    EditServerButton.Enabled = true;
			    DeleteServerButton.Enabled = true;
		    }
		    else
		    {
			    JoinServerButton.Enabled = false;
			    EditServerButton.Enabled   = false;
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
		    _toDelete = SelectedItem;
		    Alex.GameStateManager.SetActiveState(new GuiConfirmState(new GuiConfirmState.GuiConfirmStateOptions()
		    {
				MessageTranslationKey = "selectServer.deleteQuestion",
				ConfirmTranslationKey = "selectServer.deleteButton"
		    }, DeleteServerCallbackAction));
	    }

	    private GuiServerListEntryElement _toDelete;
	    private void DeleteServerCallbackAction(bool confirm)
	    {
		    if (confirm)
		    {
				RemoveItem(_toDelete);

			    _listProvider.RemoveEntry(_toDelete.SavedServerEntry);
			    //Load();
		    }
	    }

		private FastRandom Rnd = new FastRandom();
		private void OnJoinServerButtonPressed()
		{
			Task.Run(async () =>
			{
				var entry = SelectedItem.SavedServerEntry;
				var ips = Dns.GetHostAddresses(entry.Host).ToArray();
				IPAddress ip = ips[Rnd.Next(0, ips.Length - 1)];
				if (ip == null) return;

				IPEndPoint target = new IPEndPoint(ip, entry.Port);

				var authenticationService = Alex.Services.GetService<IPlayerProfileService>();
				var currentProfile = authenticationService.CurrentProfile;

				if (entry.ServerType == ServerType.Java)
				{
					if (currentProfile == null || (currentProfile.IsBedrock))
					{
						JavaLoginState loginState = new JavaLoginState(_skyBox,
							() => { Alex.ConnectToServer(target, authenticationService.CurrentProfile, false); });


						Alex.GameStateManager.SetActiveState(loginState, true);
					}
					else
					{
						Alex.ConnectToServer(target, currentProfile, false);
					}
				}
				else if (entry.ServerType == ServerType.Bedrock)
				{
					if (currentProfile == null || (!currentProfile.IsBedrock))
					{
						foreach (var profile in authenticationService.GetBedrockProfiles())
						{
							profile.IsBedrock = true;
							Log.Debug($"BEDROCK PROFILE: {profile.Username}");

							var task = await authenticationService.TryAuthenticateAsync(profile);

							if (task)
							{
								currentProfile = profile;
								break;
							}
							else
							{
								Log.Warn($"Profile auth failed.");
							}
						}
					}

					if (currentProfile == null || (!currentProfile.IsBedrock))
					{
						BEDeviceCodeLoginState loginState = new BEDeviceCodeLoginState(_skyBox,
							(profile) => { Alex.ConnectToServer(target, profile, true); });

						Alex.GameStateManager.SetActiveState(loginState, true);
					}
					else
					{
						Alex.ConnectToServer(target, currentProfile, true);
					}
				}
			});
		}

		private void OnCancelButtonPressed()
	    {
			Alex.GameStateManager.Back();
			//Alex.GameStateManager.SetActiveState("title");
	    }

	    private void OnRefreshButtonPressed()
	    {
		   /* Task.Run(() =>
		    {
			    SaveAll();
			    Load(false);
		    });*/
			PingAll(true);
	    }

	    public void Load(bool useTask = false)
	    {
		    if (useTask)
		    {
			    Task.Run(() => { LoadH(); });
		    }
		    else
		    {
			    LoadH();
		    }
	    }

	    private void LoadH()
	    {
		    _listProvider.Load();

		    ClearItems();
		    foreach (var entry in _listProvider.Data.ToArray())
		    {
			    AddItem(new GuiServerListEntryElement(entry));
		    }

		    PingAll(false);
		}

	    public void PingAll(bool forcedPing)
	    {
		    Parallel.ForEach(Items, element => element.PingAsync(forcedPing));
	    }

	    private void SaveAll()
	    {
		    foreach (var entry in _listProvider.Data.ToArray())
		    {
			    _listProvider.RemoveEntry(entry);
			}

		    foreach (var item in Items)
		    {
			    _listProvider.AddEntry(item.SavedServerEntry);
		    }
		}

	    private void AddEditServerCallbackAction(SavedServerEntry obj)
	    {
		    if (obj == null) return; //Cancelled.

		    for (var index = 0; index < Items.Length; index++)
		    {
			    var entry = Items[index];

			    if (entry.InternalIdentifier.Equals(obj.IntenalIdentifier))
			    {
					var newEntry = new GuiServerListEntryElement(obj);

				    Items[index] = newEntry;

				    newEntry.PingAsync(false);
				    break;
			    }
		    }

		    //Load();
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
			SaveAll();
	    }

	    protected override void OnUnload()
	    {
			base.OnUnload();
	    }
    }

    public class GuiServerListEntryElement : GuiSelectionListItem
    {
        private const int ServerIconSize = 32;

        public string ServerName { get;set; }
        public string ServerAddress { get; set; }

        public Texture2D ServerIcon { get; private set; }
		
        private readonly GuiTextureElement _serverIcon;
        private readonly GuiStackContainer _textWrapper;
        private readonly GuiConnectionPingIcon _pingStatus;
        
        private GuiTextElement _serverName;
        private readonly GuiTextElement _serverMotd;

	    internal SavedServerEntry SavedServerEntry;
		internal Guid InternalIdentifier = Guid.NewGuid();
	    public GuiServerListEntryElement(SavedServerEntry entry) : this(entry.ServerType == ServerType.Java ? $"§oJAVA§r - {entry.Name}" : $"§oPOCKET§r - {entry.Name}", entry.Host + ":" + entry.Port)
	    {
		    SavedServerEntry = entry;
	    }

	    private GuiServerListEntryElement(string serverName, string serverAddress)
	    {
		    SetFixedSize(355, 36);
		    
            ServerName = serverName;
            ServerAddress = serverAddress;

			Margin = new Thickness(5, 5, 5, 5);
			Padding = Thickness.One;
            Anchor = Alignment.TopFill;

            AddChild( _serverIcon = new GuiTextureElement()
            {
                Width = ServerIconSize,
                Height = ServerIconSize,
                
                Anchor = Alignment.TopLeft,

                Background = GuiTextures.DefaultServerIcon,
            });

            AddChild(_pingStatus = new GuiConnectionPingIcon()
            {
                Anchor = Alignment.TopRight,
            });

            AddChild( _textWrapper = new GuiStackContainer()
            {
                ChildAnchor = Alignment.TopFill,
				Anchor = Alignment.TopLeft
            });
			_textWrapper.Padding = new Thickness(0,0);
			_textWrapper.Margin = new Thickness(ServerIconSize + 5, 0, 0, 0);

            _textWrapper.AddChild(_serverName = new GuiTextElement()
            {
                Text = ServerName,
				Margin = Thickness.Zero
            });

            _textWrapper.AddChild(_serverMotd = new GuiTextElement()
            {
				TranslationKey = "multiplayer.status.pinging",
				Margin = new Thickness(0, 0, 5, 0),
				
				//Anchor = center
            });
        }

        public bool PingCompleted { get; private set; }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
	        if (SavedServerEntry.CachedIcon != null)
	        {
		        ServerIcon = SavedServerEntry.CachedIcon;
		        _serverIcon.Texture = ServerIcon;

	        }
			//   PingAsync();
		}

	    private GraphicsDevice _graphicsDevice = null;

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
	    {
		    _graphicsDevice = graphics.SpriteBatch.GraphicsDevice;
		    base.OnDraw(graphics, gameTime);
	    }

	    public async Task PingAsync(bool force)
	    {
		    if (PingCompleted && !force) return;
		    PingCompleted = true;

		    var hostname = ServerAddress;

			ushort port = (ushort) (SavedServerEntry.ServerType == ServerType.Java ? 25565 : 19132);// 25565;

		    var split = hostname.Split(':');
		    if (split.Length == 2)
		    {
			    if (ushort.TryParse(split[1], out port))
			    {
				    await QueryServer(split[0], port);
			    }
			    else
			    {
				    SetErrorMessage("Invalid Server Address!");
			    }
		    }
		    else if (split.Length == 1)
		    {
			    await QueryServer(split[0], port);
		    }
		    else
		    {
			    SetErrorMessage("Invalid Server Address!");
		    }
		}

        private void SetConnectingState(bool connecting)
        {
            if (connecting)
            {
                _serverMotd.TranslationKey = "multiplayer.status.pinging";
            }
            else
            {
                _serverMotd.Text = "...";
            }

            _pingStatus.SetPending();
        }

        private void SetErrorMessage(string error)
        {
           // _serverMotd.Text = error;
	        
            if (!string.IsNullOrWhiteSpace(error))
            {
	            _serverMotd.Text = error;
                _serverMotd.TranslationKey = error;
				_serverMotd.TextColor = TextColor.Red;
            }
            else
            {
                _serverMotd.TextColor = TextColor.White;
            }
            _pingStatus.SetOffline();
        }

	    private async Task QueryServer(string address, ushort port)
	    {
		    SetErrorMessage(null);
		    SetConnectingState(true);

		  //  ServerQueryResponse result;
		    var queryProvider = Alex.Instance.Services.GetService<IServerQueryProvider>();
		    if (SavedServerEntry.ServerType == ServerType.Bedrock)
		    {
			    await queryProvider.QueryBedrockServerAsync(address, port, PingCallback, QueryCompleted);//(ContinuationAction);
		    }
		    else
		    {
			    await queryProvider.QueryServerAsync(address, port, PingCallback, QueryCompleted);
		    }

		    //QueryCompleted(result);
	    }

	    private void PingCallback(ServerPingResponse response)
	    {
		    if (response.Success)
		    {
			    _pingStatus.SetPing(response.Ping);
		    }
		    else
		    {
			    _pingStatus.SetOutdated(response.ErrorMessage);
		    }
	    }

	    private static readonly Regex FaviconRegex = new Regex(@"data:image/png;base64,(?<data>.+)", RegexOptions.Compiled);
        private void QueryCompleted(ServerQueryResponse response)
        {
	        //   var response = queryTask.Result;
            SetConnectingState(false);
            
            if (response.Success)
			{
                var s = response.Status;
				var q = s.Query;
				_pingStatus.SetPlayerCount(q.Players.Online, q.Players.Max);

				if (!s.WaitingOnPing)
				{
					_pingStatus.SetPing(s.Delay);
				}

				if (q.Version.Protocol < (SavedServerEntry.ServerType == ServerType.Java ? JavaProtocol.ProtocolVersion : McpeProtocolInfo.ProtocolVersion))
				{
					if (SavedServerEntry.ServerType == ServerType.Java)
					{
						_pingStatus.SetOutdated(q.Version.Name);
					}
				}
				else if (q.Version.Protocol > (SavedServerEntry.ServerType == ServerType.Java ? JavaProtocol.ProtocolVersion : McpeProtocolInfo.ProtocolVersion))
				{
					_pingStatus.SetOutdated($"multiplayer.status.client_out_of_date", true);
				}

				if (q.Description.Extra != null)
				{
					StringBuilder builder = new StringBuilder();
					foreach (var extra in q.Description.Extra)
					{
						if (extra.Color != null)
							builder.Append(API.Utils.TextColor.GetColor(extra.Color).ToString());

						if (extra.Bold.HasValue)
						{
							builder.Append(ChatFormatting.Bold);
						}

						if (extra.Italic.HasValue)
						{
							builder.Append(ChatFormatting.Italic);
						}

						if (extra.Underlined.HasValue)
						{
							builder.Append(ChatFormatting.Underline);
						}

						if (extra.Strikethrough.HasValue)
						{
							builder.Append(ChatFormatting.Strikethrough);
						}

						if (extra.Obfuscated.HasValue)
						{
							builder.Append(ChatFormatting.Obfuscated);
						}

						builder.Append(extra.Text);
					}

					_serverMotd.Text = builder.ToString();
				}
				else
				{
					_serverMotd.Text = q.Description.Text;
				}

				if (!string.IsNullOrWhiteSpace(q.Favicon))
	            {
		            var match = FaviconRegex.Match(q.Favicon);
		            if (match.Success && _graphicsDevice != null)
		            {
                        AutoResetEvent reset = new AutoResetEvent(false);
                        Alex.Instance.UIThreadQueue.Enqueue(() =>
                        {
                            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(match.Groups["data"].Value)))
                            {
                                ServerIcon = GpuResourceManager.GetTexture2D(this, _graphicsDevice, ms);
                            }

                            reset.Set();
                        });

                        reset.WaitOne();

			            SavedServerEntry.CachedIcon = ServerIcon;
			            _serverIcon.Texture = ServerIcon;
		            }
	            }
            }
            else
            {
                SetErrorMessage(response.ErrorMessage);
            }

        }
    }
}
