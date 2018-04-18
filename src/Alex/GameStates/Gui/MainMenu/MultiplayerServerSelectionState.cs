using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Icons;
using Alex.API.Gui.Rendering;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.GameStates.Gui.Common;
using Alex.GameStates.Gui.Elements;
using Alex.Graphics.Gui.Elements;
using Alex.Networking.Java;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.GameStates.Gui.MainMenu
{
    public class MultiplayerServerSelectionState : ListSelectionStateBase<GuiServerListEntryElement>
    {
		private GuiContainer Footer { get; }
		public MultiplayerServerSelectionState() : base()
        {
	        Title = "Multiplayer";

	        Gui.AddChild(Footer = new GuiContainer()
	        {
		        Height = 42,
				Anchor = Alignment.BottomFill
	        });

            Footer.AddChild(new GuiBeaconButton("Direct Connect", () => Alex.GameStateManager.SetActiveState<MultiplayerConnectState>())
            {
                Anchor = Alignment.BottomCenter,
                Y = -25
            });

	        AddItem(new GuiServerListEntryElement("Localhost", "localhost:25565"));
	        AddItem(new GuiServerListEntryElement("Hypixel", "mc.hypixel.net:25565"));
		}

    }

    public class GuiServerListEntryElement : GuiContainer
    {
        private const int ServerIconSize = 32;

        public string ServerName { get;set; }
        public string ServerAddress { get; set; }

        public Texture2D ServerIcon { get; private set; }
        public string RawMotd { get; private set; }

        public byte PingQuality { get; private set; }

        public bool IsPingPending { get; private set; }


        private GuiTextureElement _serverIcon;
        private GuiStackContainer _textWrapper;
        private GuiConnectionPingIcon _pingStatus;
        
        private GuiTextElement _serverName;
        private GuiTextElement _serverMotd;

	   // public override int Width => 325;
	   
	    public GuiServerListEntryElement(string serverName, string serverAddress)
        {
            ServerName = serverName;
            ServerAddress = serverAddress;
	        Width = 325;

            Anchor = Alignment.TopFill;

            AddChild( _serverIcon = new GuiTextureElement()
            {
                Width = ServerIconSize,
                Height = ServerIconSize,
                
                Anchor = Alignment.TopLeft,

                DefaultBackgroundTexture = GuiTextures.DefaultServerIcon
            });

            AddChild(_pingStatus = new GuiConnectionPingIcon()
            {
                Anchor = Alignment.TopRight,
            });

            AddChild( _textWrapper = new GuiStackContainer()
            {
                X = ServerIconSize + 5,
                ChildAnchor = Alignment.TopFill
                //HorizontalContentAlignment = HorizontalAlignment.FillParent,
                //VerticalContentAlignment = VerticalAlignment.Top
            });

            _textWrapper.AddChild(_serverName = new GuiTextElement()
            {
                Text = ServerName,
				
            });
            _textWrapper.AddChild(_serverMotd = new GuiTextElement()
            {
				Text = "Pinging server..."
            });


        }

        public bool PingCompleted { get; private set; }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
			
            Ping();
        }

	    private GraphicsDevice _graphicsDevice = null;
	    protected override void OnDraw(GuiRenderArgs args)
	    {
		    _graphicsDevice = args.Graphics;
		    base.OnDraw(args);
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
        
        private void QueryServer(string address, ushort port)
        {
            SetErrorMessage(null);
            SetConnectingState(true);

            var queryProvider = Alex.Instance.Services.GetService<IServerQueryProvider>();
            queryProvider.QueryServerAsync(address, port).ContinueWith(ContinuationAction);
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
        
		private static readonly Regex FaviconRegex = new Regex(@"data:image/png;base64,(?<data>.+)", RegexOptions.Compiled);
        private void ContinuationAction(Task<ServerQueryResponse> queryTask)
        {
            var response = queryTask.Result;
            SetConnectingState(false);
            
            if (response.Success)
			{
                var s = response.Status;
				_pingStatus.SetPlayerCount(s.NumberOfPlayers, s.MaxNumberOfPlayers);
                _pingStatus.SetPing(s.Delay);

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
		            if (match.Success)
		            {
			         //   try
			            {
				            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(match.Groups["data"].Value)))
				            {
					            ServerIcon = Texture2D.FromStream(_graphicsDevice, ms);
				            }

				            _serverIcon.Texture = ServerIcon;

			            }
			         //   catch
			            {
							
			            }
		            }
	            }
            }
            else
            {
                SetErrorMessage(response.ErrorMessage);
            }

        }

        private void ServerPingCallback(string rawMotd, long pingMs)
        {
            RawMotd = rawMotd;
            
            _serverMotd.Text = rawMotd;
        }
    }
}
