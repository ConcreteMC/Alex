using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Alex.API.Data.Servers;
using Alex.API.Graphics;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using Alex.Gamestates.Login;
using Alex.Gui;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.Gamestates.Multiplayer
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

			_listProvider = GetService<IListStorageProvider<SavedServerEntry>>();

		    Title = "Multiplayer";
		    TitleTranslationKey = "multiplayer.title";
		    
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
		    
		    var queryProvider = GetService<IServerQueryProvider>();
		    
		    _listProvider.Load();
		    
		    ClearItems();

		    List<Task> tasks = new List<Task>();
		    foreach (var entry in _listProvider.Data.ToArray())
		    {
			    var element = new GuiServerListEntryElement(queryProvider, entry);
			    AddItem(element);
			    
			    tasks.Add(element.PingAsync(false));
		    }
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
		    Alex.UIThreadQueue.Enqueue(() =>
		    {
			    if (confirm)
			    {
				    RemoveItem(_toDelete);

				    _listProvider.RemoveEntry(_toDelete.SavedServerEntry);
				    //Load();
			    }
		    });
	    }

		private FastRandom Rnd = new FastRandom();

		private async void OnJoinServerButtonPressed()
		{
			var       entry = SelectedItem.SavedServerEntry;
			var       ips   = Dns.GetHostAddresses(entry.Host).ToArray();
			IPAddress ip    = ips[Rnd.Next(0, ips.Length - 1)];

			if (ip == null) return;

			IPEndPoint target = new IPEndPoint(ip, entry.Port);

			var authenticationService = GetService<IPlayerProfileService>();
			var currentProfile        = authenticationService.CurrentProfile;

			if (entry.ServerType == ServerType.Java)
			{
				if (currentProfile == null || (currentProfile.IsBedrock))
				{
					JavaLoginState loginState = new JavaLoginState(
						_skyBox,
						() =>
						{
							Alex.ConnectToServer(
								target, authenticationService.CurrentProfile, false,
								SelectedItem.SavedServerEntry.Host);
						});


					Alex.GameStateManager.SetActiveState(loginState, true);
				}
				else
				{
					Alex.ConnectToServer(target, currentProfile, false, SelectedItem.SavedServerEntry.Host);
				}
			}
			else if (entry.ServerType == ServerType.Bedrock)
			{
				if (SelectedItem.ConnectionEndpoint != null)
				{
					target = SelectedItem.ConnectionEndpoint;
				}

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

				if ((currentProfile == null || (!currentProfile.IsBedrock)) || !currentProfile.Authenticated)
				{
					BEDeviceCodeLoginState loginState = new BEDeviceCodeLoginState(
						_skyBox, (profile) => { Alex.ConnectToServer(target, profile, true); });

					Alex.GameStateManager.SetActiveState(loginState, true);
				}
				else
				{
					Alex.ConnectToServer(target, currentProfile, true);
				}
			}
		}

		private void OnCancelButtonPressed()
	    {
			Alex.GameStateManager.Back();
			//Alex.GameStateManager.SetActiveState("title");
	    }

	    private void OnRefreshButtonPressed()
	    {
		    foreach (var item in Items)
		    {
			    item.PingAsync(true);
		    }
		   /* Task.Run(() =>
		    {
			    SaveAll();
			    Load(false);
		    });*/
		//	PingAll(true);
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
		    
		    /*Alex.UIThreadQueue.Enqueue(() =>
		    {
			    
		    });*/
	    }

	    private void AddEditServerCallbackAction(SavedServerEntry obj)
	    {
		    var queryProvider = GetService<IServerQueryProvider>();
		    
		    if (obj == null) return; //Cancelled.

		    for (var index = 0; index < Items.Length; index++)
		    {
			    var entry = Items[index];

			    if (entry.InternalIdentifier.Equals(obj.IntenalIdentifier))
			    {
					var newEntry = new GuiServerListEntryElement(queryProvider, obj);

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
}
