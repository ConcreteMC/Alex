using Alex.Common.Graphics;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Utils;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gamestates.Multiplayer
{
	public class UserSelectionState : ListSelectionStateBase<UserSelectionItem>
	{
		public delegate void ProfileSelected(PlayerProfile selectedProfile);
		public delegate void Cancelled();
		public delegate void AddAccountButtonClicked();
		
		private AlexButton SelectButton { get; set; }
		private AlexButton AddButton { get; set; }
		private AlexButton RemoveButton { get; set; }
		
		private GuiPanoramaSkyBox _skyBox;
		public bool AllowDelete { get; set; } = true;
		public UserSelectionState(ServerTypeImplementation serverType, GuiPanoramaSkyBox skyBox)
		{
			Title = $"{serverType.DisplayName} - Account Selection";
			_skyBox = skyBox;
			Background = new GuiTexture2D(skyBox, TextureRepeatMode.Stretch);

			Footer.AddRow(row =>
			{
				row.AddChild(SelectButton = new AlexButton("Select Account",
					SelectAccountClicked)
				{
					Enabled = false
				});
				
				row.AddChild(AddButton = new AlexButton("Add Account",
					AddAccountClicked)
				{
					Enabled = true
				});
			});

			Footer.AddRow(
				row =>
				{
					row.AddChild(
						RemoveButton = new AlexButton("Remove", OnRemoveClicked)
						{
							Enabled = false
						});
					row.AddChild(new GuiBackButton()
					{
						TranslationKey = "gui.cancel"
					});
				});
		}

		public void ReloadData(PlayerProfile[] availableProfiles)
		{
			ClearItems();
			foreach (var profile in availableProfiles)
			{
				AddItem(new UserSelectionItem(profile));
			}
		}
		
		private void OnRemoveClicked()
		{
			var item = SelectedItem;
			var selectedProfile = item?.Profile;

			if (selectedProfile == null)
				return;
			
			var profileManager = GetService<ProfileManager>();

			if (profileManager != null)
			{
				RemoveItem(item);
				profileManager.RemoveProfile(selectedProfile);
			}
		}

		private void AddAccountClicked()
		{
			OnAddAccount?.Invoke();
		}

		private void SelectAccountClicked()
		{
			SelectAccount(SelectedItem);
		}

		private void SelectAccount(UserSelectionItem item)
		{
			if (item?.Profile != null)
			{
				Alex.GameStateManager.Back();
				OnProfileSelection?.Invoke(item?.Profile);
			}
		}

		public ProfileSelected OnProfileSelection;
		public Cancelled OnCancel;
		public AddAccountButtonClicked OnAddAccount;

		/// <inheritdoc />
		protected override void OnHide()
		{
			base.OnHide();
			OnCancel?.Invoke();
		}

		/// <inheritdoc />
		protected override void OnSelectedItemChanged(UserSelectionItem newItem)
		{
			base.OnSelectedItemChanged(newItem);
			if (newItem == null)
			{
				SelectButton.Enabled = false;
				RemoveButton.Enabled = false;
				return;
			}
			
			SelectButton.Enabled = true;
			RemoveButton.Enabled = AllowDelete;
		}

		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			_skyBox?.Update(gameTime);
			base.OnUpdate(gameTime);
		}

		/// <inheritdoc />
		protected override void OnDraw(IRenderArgs args)
		{
			var sb = _skyBox;

			if (sb != null)
			{
				if (!sb.Loaded)
				{
					sb.Load(Alex.GuiRenderer);
				}
				sb.Draw(args);
			}

			base.OnDraw(args);
		}
		
		protected override void OnItemDoubleClick(UserSelectionItem item)
		{
			base.OnItemDoubleClick(item);
		    
			if (SelectedItem != item)
				return;
		    
			SelectAccount(item);
		}
	}
}