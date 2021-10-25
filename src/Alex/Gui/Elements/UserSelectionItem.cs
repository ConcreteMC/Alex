using System;
using Alex.Common.Gui.Graphics;
using Alex.Common.Services;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using RocketUI;

namespace Alex.Gui.Elements
{
	public class UserSelectionItem : SelectionListItem
	{
		private TextureElement _playerHead;
		private StackContainer _textWrapper;
		
		public PlayerProfile Profile { get; }
		public UserSelectionItem(PlayerProfile profile)
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

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
		}
	}
}