using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Alex.API.Data.Servers;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Icons;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.GameStates.Gui.Common;
using Alex.Graphics.Gui.Elements;
using Alex.Gui;
using Alex.Networking.Java;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.GameStates.Gui.Multiplayer
{
    public class MultiplayerServerSelectionState : ListSelectionStateBase<GuiServerListEntryElement>
    {
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
		    Load();
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
		    Alex.GameStateManager.SetActiveState(new MultiplayerAddEditServerState(AddEditServerCallbackAction));
	    }

	    private void OnEditItemButtonPressed()
	    {
		    Alex.GameStateManager.SetActiveState(new MultiplayerAddEditServerState(SelectedItem.SavedServerEntry, AddEditServerCallbackAction));
	    }
		
	    private void OnDeleteItemButtonPressed()
	    {
		    _toDelete = SelectedItem.SavedServerEntry;
		    Alex.GameStateManager.SetActiveState(new GuiConfirmState(new GuiConfirmState.GuiConfirmStateOptions()
		    {
				MessageTranslationKey = "selectServer.deleteQuestion",
				ConfirmTranslationKey = "selectServer.deleteButton"
		    }, DeleteServerCallbackAction));
	    }

	    private SavedServerEntry _toDelete;
	    private void DeleteServerCallbackAction(bool confirm)
	    {
		    if (confirm)
		    {
			    _listProvider.RemoveEntry(_toDelete);
			    Load();
		    }
	    }

	    private void OnJoinServerButtonPressed()
	    {
		    var entry = SelectedItem.SavedServerEntry;
		    var ip = Dns.GetHostAddresses(entry.Host).FirstOrDefault();

		    Alex.ConnectToServer(new IPEndPoint(ip, entry.Port));
	    }
		
	    private void OnCancelButtonPressed()
	    {
			Alex.GameStateManager.Back();
		//	Alex.GameStateManager.SetActiveState<TitleState>();
	    }

	    private void OnRefreshButtonPressed()
	    {
			Load();
	    }

	    public void Load()
	    {
		    _listProvider.Load();
		
			ClearItems();
		    foreach (var entry in _listProvider.Data.ToArray())
		    {
				AddItem(new GuiServerListEntryElement(entry));
		    }

		    PingAll();
	    }

	    public void PingAll()
	    {
		    foreach (var item in Items)
		    {
				item.Ping();
		    }
	    }


	    private void AddEditServerCallbackAction(SavedServerEntry obj)
	    {
		    //if (obj == null) return;

			Load();
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
    }

    public class GuiServerListEntryElement : GuiSelectionListItem
    {
        private const int ServerIconSize = 32;

        public string ServerName { get;set; }
        public string ServerAddress { get; set; }

        public Texture2D ServerIcon { get; private set; }
        public string RawMotd { get; private set; }

        public byte PingQuality { get; private set; }

        public bool IsPingPending { get; private set; }
		
        private readonly GuiTextureElement _serverIcon;
        private readonly GuiStackContainer _textWrapper;
        private readonly GuiConnectionPingIcon _pingStatus;
        
        private GuiTextElement _serverName;
        private readonly GuiTextElement _serverMotd;

	    internal SavedServerEntry SavedServerEntry;
		
	    public GuiServerListEntryElement(SavedServerEntry entry) : this(entry.Name, entry.Host + ":" + entry.Port)
	    {
		    SavedServerEntry = entry;
	    }

	    private GuiServerListEntryElement(string serverName, string serverAddress)
	    {
		    Width = 335;
		    MaxWidth = 335;

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
				Text = "Pinging server...",
				Margin = new Thickness(0, 0, 5, 0),
				
				//Anchor = center
            });
        }

        public bool PingCompleted { get; private set; }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
			
            Ping();
        }

	    private GraphicsDevice _graphicsDevice = null;

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
	    {
		    _graphicsDevice = graphics.SpriteBatch.GraphicsDevice;
		    base.OnDraw(graphics, gameTime);
	    }

	    public void Ping()
        {
            if (PingCompleted) return;
            PingCompleted = true;

            
            var hostname = ServerAddress;

            ushort port = 25565;

            var split = hostname.Split(':');
            if (split.Length == 2)
            {
                if (ushort.TryParse(split[1], out port))
                {
                    QueryServer(split[0], port);
                }
                else
                {
                    SetErrorMessage("Invalid Server Address!");
                }
            }
            else if (split.Length == 1)
            {
                QueryServer(split[0], port);
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
                _serverMotd.Text = "Pinging Server...";
            }
            else
            {
                _serverMotd.Text = "...";
            }

            _pingStatus.SetPending();
        }

        private void SetErrorMessage(string error)
        {
            _serverMotd.Text = error;

            if (!string.IsNullOrWhiteSpace(error))
            {
                _serverMotd.TextColor = TextColor.Red;
            }
            else
            {
                _serverMotd.TextColor = TextColor.White;
            }
            _pingStatus.SetOffline();
        }

	    private void QueryServer(string address, ushort port)
	    {
		    SetErrorMessage(null);
		    SetConnectingState(true);

		    var queryProvider = Alex.Instance.Services.GetService<IServerQueryProvider>();
		    queryProvider.QueryServerAsync(address, port, PingCallback).ContinueWith(QueryCompleted);
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
        private void QueryCompleted(Task<ServerQueryResponse> queryTask)
        {
            var response = queryTask.Result;
            SetConnectingState(false);
            
            if (response.Success)
			{
                var s = response.Status;
				_pingStatus.SetPlayerCount(s.NumberOfPlayers, s.MaxNumberOfPlayers);

				if (!s.WaitingOnPing)
				{
					_pingStatus.SetPing(s.Delay);
				}

				if (s.ProtocolVersion < JavaProtocol.ProtocolVersion)
				{
					_pingStatus.SetOutdated(s.Version);
				}
				else if (s.ProtocolVersion > JavaProtocol.ProtocolVersion)
				{
					_pingStatus.SetOutdated($"Client out of date!");
				}

	            _serverMotd.Text = s.Motd;

	            if (!string.IsNullOrWhiteSpace(s.FaviconDataRaw))
	            {
		            var match = FaviconRegex.Match(s.FaviconDataRaw);
		            if (match.Success && _graphicsDevice != null)
		            {
			            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(match.Groups["data"].Value)))
			            {
				            ServerIcon = Texture2D.FromStream(_graphicsDevice, ms);
			            }

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
