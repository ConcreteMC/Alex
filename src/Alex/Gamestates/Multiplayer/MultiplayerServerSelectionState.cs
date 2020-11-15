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
using Alex.Gui.Elements;
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
				    TranslationKey = "selectServer.direct",
				    Enabled = false
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
		    
		  //  var queryProvider = GetService<IServerQueryProvider>();
		    
		    _listProvider.Load();

		    Reload();
	    }

	    private void Reload()
	    {
		    ClearItems();
		    
		    Task previousTask = null;
		    foreach (var entry in _listProvider.Data.ToArray())
		    {
			    if (Alex.ServerTypeManager.TryGet(entry.ServerType, out var typeImplementation))
			    {
				    var element = new GuiServerListEntryElement(typeImplementation, entry);
				    AddItem(element);

				    if (previousTask != null)
				    {
					    previousTask = previousTask.ContinueWith(r => element.PingAsync(false));
				    }
				    else
				    {
					    previousTask = element.PingAsync(false);
				    }
			    }
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
			var overlay = new LoadingOverlay();
			Alex.GuiManager.AddScreen(overlay);

			try
			{
				var       entry = SelectedItem.SavedServerEntry;
				var       ips   = Dns.GetHostAddresses(entry.Host).ToArray();
				IPAddress ip    = ips[Rnd.Next(0, ips.Length - 1)];

				if (ip == null) return;

				IPEndPoint target = new IPEndPoint(ip, entry.Port);

				var           authenticationService = GetService<IPlayerProfileService>();
				//var currentProfile        = authenticationService.CurrentProfile;

				if (Alex.ServerTypeManager.TryGet(entry.ServerType, out var typeImplementation))
				{
					var           profiles       = authenticationService.GetProfiles(entry.ServerType);
					PlayerProfile currentProfile = null;

					if (profiles.Length == 1)
					{
						currentProfile = profiles[0];
					}
					
					void Connect()
					{
						Alex.ConnectToServer(
							typeImplementation, new ServerConnectionDetails(target, entry.Host),
							authenticationService.CurrentProfile);
					
						Alex.GuiManager.RemoveScreen(overlay);
						overlay = null;
					}
					
					if (currentProfile == null || !await typeImplementation.VerifyAuthentication(currentProfile))
					{
						await typeImplementation.Authenticate(
							_skyBox, currentProfile, result =>
							{
								if (result)
								{
									Connect();
								}
							});
					}
					else
					{
						Connect();
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
		    Task previousTask = null;
		    foreach (var item in Items)
		    {
			    var i = item;
			   if (previousTask == null)
				   previousTask =  i.PingAsync(true);
			   else
			   {
				   previousTask = previousTask.ContinueWith(x => i.PingAsync(true));
			   }
		    }
	    }

	    private void SaveAll()
	    {
		    _listProvider.Save(_listProvider.Data);
		  /*  foreach (var entry in _listProvider.Data.ToArray())
		    {
			    _listProvider.RemoveEntry(entry);
		    }

		    foreach (var item in Items)
		    {
			    _listProvider.AddEntry(item.SavedServerEntry);
		    }*/
		    
		    /*Alex.UIThreadQueue.Enqueue(() =>
		    {
			    
		    });*/
	    }

	    private void AddEditServerCallbackAction(MultiplayerAddEditServerState.AddOrEditCallback obj)
	    {
		    if (obj == null) return; //Cancelled.

		    if (!obj.IsNew)
		    {
			    for (var index = 0; index < Items.Length; index++)
			    {
				    var entry = Items[index];

				    if (entry.SavedServerEntry.InternalIdentifier.Equals(obj.Entry.InternalIdentifier))
				    {
					    var newEntry = new GuiServerListEntryElement(entry.ServerTypeImplementation, obj.Entry);

					    Items[index] = newEntry;

					    newEntry.PingAsync(false);

					    _listProvider.RemoveEntry(entry.SavedServerEntry);
					    _listProvider.AddEntry(obj.Entry);
					    _listProvider.MoveEntry(entry.SavedServerEntry.ListIndex, obj.Entry);
					    
					    break;
				    }
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
			SaveAll();
	    }

	    protected override void OnUnload()
	    {
			base.OnUnload();
	    }
    }
}
