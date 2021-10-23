using System;
using Alex.Common.Graphics;
using Alex.Common.Gui.Elements;
using Alex.Common.Gui.Graphics;
using Alex.Common.Services;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Utils;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using RocketUI;

namespace Alex.Gamestates.Multiplayer
{
	public class ProfileItem : SelectionListItem
	{
		private TextureElement _playerHead;
		private StackContainer _textWrapper;
		
		public PlayerProfile Profile { get; }
		public ProfileItem(PlayerProfile profile)
		{
			Profile = profile;
			
			SetFixedSize(355, 36);

			Margin = new Thickness(5, 5, 5, 5);
			Padding = Thickness.One;
			Anchor = Alignment.TopFill;
			
			AddChild(_playerHead = new TextureElement()
			{
				Width = 32,
				Height = 32,
                
				Anchor = Alignment.TopLeft,
				BackgroundOverlay = Color.White * 0.3f,
				//Texture = AlexGuiTextures.PlayerHead
				Background = AlexGuiTextures.UnknownPackIcon,
			});

			bool isGreen = profile.Authenticated;

			if (!isGreen && profile.ExpiryTime.HasValue && profile.ExpiryTime > DateTime.UtcNow)
			{
				isGreen = true;
			}
			
			TextureElement authenticatedElement;
			AddChild(authenticatedElement = new TextureElement()
			{
				Anchor = Alignment.TopRight,
				Background = isGreen ?  AlexGuiTextures.GreenCheckMark :  AlexGuiTextures.GreyCheckMark,
				Width = 10,
				Height = 10,
				AutoSizeMode = AutoSizeMode.None
			});  
		//	authenticatedElement.SetFixedSize(10, 8);
			
			AddChild( _textWrapper = new StackContainer()
			{
				ChildAnchor = Alignment.TopFill,
				Anchor = Alignment.TopLeft
			});
			_textWrapper.Padding = new Thickness(0,0);
			_textWrapper.Margin = new Thickness(32 + 5, 0, 0, 0);

			_textWrapper.AddChild(new TextElement()
			{
				Text = profile.PlayerName,
				Margin = Thickness.Zero
			});

			_textWrapper.AddChild(new TextElement()
			{
				Text = $"{ChatColors.Gray}{(profile.Username)}",
				Margin = new Thickness(0, 0, 5, 0),
				IsVisible = !string.Equals(profile.PlayerName, profile.Username, StringComparison.InvariantCultureIgnoreCase)
			});
			
			_textWrapper.AddChild(new TextElement()
			{
				Text = $"{ChatColors.Red}{(profile.AuthError ?? string.Empty)}",
				Margin = new Thickness(0, 0, 5, 0)
			});
		}
	}
	
	public class UserSelectionState : ListSelectionStateBase<ProfileItem>
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
				AddItem(new ProfileItem(profile));
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
			Alex.GameStateManager.Back();
			OnProfileSelection?.Invoke(SelectedItem?.Profile);
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
		protected override void OnSelectedItemChanged(ProfileItem newItem)
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
	}
}