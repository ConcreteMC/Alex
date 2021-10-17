using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Data.Servers;
using Alex.Common.Gui.Elements.Icons;
using Alex.Common.Gui.Graphics;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gui.Elements;
using Alex.Networking.Java;
using Alex.Services;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Net;
using MiNET.Utils;
using RocketUI;
using GpuResourceManager = Alex.Common.Graphics.GpuResources.GpuResourceManager;

namespace Alex.Gamestates.Multiplayer
{
	public class GuiServerListEntryElement : SelectionListItem
	{
		private const int ServerIconSize = 32;

		private IPEndPoint _connectionEndPoint = null;
		public IPEndPoint ConnectionEndpoint
		{
			get
			{
				if (_connectionEndPoint != null)
					return _connectionEndPoint;
				
				var resolved = JavaServerQueryProvider.ResolveHostnameAsync(SavedServerEntry.Host).Result;

				return new IPEndPoint(resolved.Result, SavedServerEntry.Port);
			}
			set
			{
				_connectionEndPoint = value;
			}
		}

		private readonly GuiMoveServerElement _serverIcon;
		private readonly StackContainer     _textWrapper;
		private readonly GuiConnectionPingIcon _pingStatus;
        
		private          TextElement _serverName;
		private readonly TextElement _serverMotd;

		internal SavedServerEntry SavedServerEntry;
		public bool                     SaveEntry                { get; set; } = true;
		public bool CanDelete { get; set; } = true;
		public GuiServerListEntryElement(ServerTypeImplementation serverTypeImplementation, SavedServerEntry entry) : this(
			serverTypeImplementation.QueryProvider,
			$"{entry.Name}")
		{
			SavedServerEntry = entry;
		}

		private IServerQueryProvider QueryProvider { get; }
	//	private CancellationTokenSource _cancellationTokenSource;
		private GuiServerListEntryElement(IServerQueryProvider queryProvider, string serverName)
		{
			QueryProvider = queryProvider;
			SetFixedSize(355, 36);

			Margin = new Thickness(5, 5, 5, 5);
			Padding = Thickness.One;
			Anchor = Alignment.TopFill;

			AddChild(_serverIcon = new GuiMoveServerElement()
			{
				Width = ServerIconSize,
				Height = ServerIconSize,

				Anchor = Alignment.TopLeft,

			  //Background = AlexGuiTextures.DefaultServerIcon,
			});

			AddChild(_pingStatus = new GuiConnectionPingIcon()
			{
				Anchor = Alignment.TopRight,
			});

			AddChild( _textWrapper = new StackContainer()
			{
				ChildAnchor = Alignment.TopFill,
				Anchor = Alignment.TopLeft
			});
			_textWrapper.Padding = new Thickness(0,0);
			_textWrapper.Margin = new Thickness(ServerIconSize + 5, 0, 0, 0);

			_textWrapper.AddChild(_serverName = new TextElement()
			{
				Text = serverName,
				Margin = Thickness.Zero
			});

			_textWrapper.AddChild(_serverMotd = new TextElement()
			{
				TranslationKey = PingTranslationKey,
				Margin = new Thickness(0, 0, 5, 0),
				
				//Anchor = center
			});
		}



		private const string PingTranslationKey = "multiplayer.status.pinging";
		
		public bool PingCompleted { get; private set; }

		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
			
			if (SavedServerEntry.CachedIcon != null)
			{
				_serverIcon.Texture = SavedServerEntry.CachedIcon;
			}
			else
			{
				_serverIcon.Texture = renderer.GetTexture(AlexGuiTextures.DefaultServerIcon);
			}
			//   PingAsync();
		}

		private GraphicsDevice _graphicsDevice = null;

		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			_graphicsDevice = graphics.SpriteBatch.GraphicsDevice;
			base.OnDraw(graphics, gameTime);
		}

		private bool _pingComplete = false;
		public async Task PingAsync(bool force, CancellationToken cancellationToken)
		{
			if (PingCompleted && !force) return;
			PingCompleted = true;

			var hostname = SavedServerEntry.Host;
			ushort port = SavedServerEntry.Port;
			
			await QueryServer(hostname, port, cancellationToken);
		}

		private void SetConnectingState(bool connecting)
		{
			if (connecting)
			{
				_serverMotd.TranslationKey = PingTranslationKey;
			}
			else
			{
				_serverMotd.Text = "...";
				_serverMotd.TranslationKey = null;
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
				_serverMotd.TextColor = (Color) TextColor.Red;
			}
			else
			{
				_serverMotd.TextColor = (Color) TextColor.White;
			}
			_pingStatus.SetOffline();
		}

		private int _queryAttempt = 0;
		private async Task QueryServer(string address, ushort port, CancellationToken cancellationToken)
		{
			SetErrorMessage(null);
			SetConnectingState(true);

			//  ServerQueryResponse result;
			// var queryProvider = Alex.GetService<IServerQueryProvider>();
			/*if (SavedServerEntry.ServerType == ServerType.Bedrock)
			{
				await QueryProvider.Q(address, port, PingCallback, QueryCompleted);//(ContinuationAction);
			}
			else
			{*/
			var resolved = await JavaServerQueryProvider.ResolveHostnameAsync(address);

			if (!resolved.Success)
			{
				QueryCompleted(new ServerQueryResponse(false, "multiplayer.status.cannot_resolve", new ServerQueryStatus()
				{
					Delay = 0,
					Success = false,

					EndPoint = null,
					Address = address,
					Port = port
				}));
			}
			else
			{
				var endPoint = new IPEndPoint(resolved.Results[_queryAttempt++ % resolved.Results.Length], port);
				
				await QueryProvider.QueryServerAsync(
					new ServerConnectionDetails(endPoint, address), PingCallback,
					QueryCompleted, cancellationToken);
			}
			//}

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

				ConnectionEndpoint = s.EndPoint;

				_pingStatus.SetVersion(!string.IsNullOrWhiteSpace(q.Version.Name) ? q.Version.Name : q.Version.Protocol.ToString());
				
				if (!s.WaitingOnPing)
				{
					_pingStatus.SetPing(s.Delay);
				}
				
				switch (q.Version.Compatibility)
				{
					case CompatibilityResult.OutdatedClient:
						//_pingStatus.SetOutdated($"multiplayer.status.client_out_of_date", true);
						_pingStatus.SetOutdated($"Client out of date! (#{q.Version.Protocol})");
						break;

					case CompatibilityResult.OutdatedServer:
						if (!string.IsNullOrWhiteSpace(q.Version.Name))
						{
							_pingStatus.SetOutdated(q.Version.Name);
						}
						else
						{
							_pingStatus.SetOutdated($"Server out of date! (#{q.Version.Protocol})");
						}

						break;

					case CompatibilityResult.Unknown:
						break;
				}

				if (q.Description.Extra != null)
				{
					StringBuilder builder = new StringBuilder();
					foreach (var extra in q.Description.Extra)
					{
						if (extra.Color != null)
							builder.Append(TextColor.GetColor(extra.Color).ToString());

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
					_serverMotd.TranslationKey = null;
				}
				else
				{
					_serverMotd.Text = q.Description.Text;
					_serverMotd.TranslationKey = null;
				}

				if (!string.IsNullOrWhiteSpace(q.Favicon))
				{
					var match = FaviconRegex.Match(q.Favicon);
					if (match.Success && _graphicsDevice != null)
					{
						AutoResetEvent reset = new AutoResetEvent(false);
						Alex.Instance.UiTaskManager.Enqueue(() =>
						{
							using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(match.Groups["data"].Value)))
							{
								_serverIcon.Texture = SavedServerEntry.CachedIcon = Texture2D.FromStream(_graphicsDevice, ms);
							}

							reset.Set();
						});
					}
				}
			}
			else
			{
				SetErrorMessage(response.ErrorMessage);
			}

		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			
			if (disposing)
			{
				//ServerIcon?.Dispose();
			}
		}
	}
}