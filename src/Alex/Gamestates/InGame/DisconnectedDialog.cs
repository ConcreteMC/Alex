using System;
using System.Net;
using Alex.Common;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.MainMenu;
using Alex.Gui;
using Alex.Interfaces;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gamestates.InGame
{
	public record ConnectionInfo(IPEndPoint EndPoint, string ServerType, string Hostname, PlayerProfile Profile);

	public class DisconnectedDialog : DialogBase
	{
		private readonly Alex _game;

		//private static DisconnectedDialog _activeDialog = null;
		public TextElement DisconnectedTextElement { get; private set; }

		private StackContainer Header { get; }
		private MultiStackContainer Body { get; }

		public EventHandler OnDialogClosed;
		private ConnectionInfo _connectionInfo = null;

		private ConnectionInfo ConnectionInfo
		{
			get => _connectionInfo;
			set
			{
				_connectionInfo = value;
				_reconnectButton.Enabled = value != null;
			}
		}

		private AlexButton _reconnectButton = null;

		public DisconnectedDialog(Alex game)
		{
			_game = game;
			Anchor = Alignment.Fill;
			ContentContainer.Anchor = Alignment.Fill;
			BackgroundOverlay = new Color(Color.Black, 0.65f);

			//	TitleTranslationKey = "multiplayer.disconnect.generic";

			ContentContainer.AddChild(
				Header = new StackContainer()
				{
					Height = 32,
					Padding = new Thickness(5),
					Margin = new Thickness(5),
					Anchor = Alignment.TopFill,
					ChildAnchor = Alignment.MiddleCenter
				});

			Header.AddChild(
				new TextElement()
				{
					TranslationKey = "multiplayer.disconnect.generic",
					FontStyle = FontStyle.Bold | FontStyle.DropShadow,
					TextColor = (Color)TextColor.White.ToXna(),
					Anchor = Alignment.MiddleCenter
				});

			ContentContainer.AddChild(
				Body = new MultiStackContainer(
					row =>
					{
						row.Anchor = Alignment.BottomFill;
						//row.Orientation = Orientation.Horizontal;
						row.ChildAnchor = Alignment.BottomFill;
						//row.Margin = new Thickness(3);
						row.Width = 356;
						row.MaxWidth = 356;
						row.Margin = new Thickness(0, 10, 0, 10);
					})
				{
					Orientation = Orientation.Vertical,
					Anchor = Alignment.Fill,
					ChildAnchor = Alignment.MiddleCenter,
					Margin = new Thickness(5, 42, 5, 5)
				});

			Body.AddRow(
				row =>
				{
					row.ChildAnchor = Alignment.FillCenter;

					row.AddChild(
						DisconnectedTextElement = new TextElement()
						{
							MaxWidth = 356,
							Width = 356,
							MinWidth = 356,
							TranslationKey = "disconnect.lost",
							TextColor = (Color)TextColor.Red.ToXna(),
							Anchor = Alignment.MiddleCenter,
							Wrap = true,
							TextAlignment = TextAlignment.Center
						});
				});

			Body.AddRow(
				_reconnectButton =
					new AlexButton(ReconnectButtonClicked) { Text = "Reconnect", Enabled = ConnectionInfo != null }
					   .ApplyModernStyle(false),
				new AlexButton(MenuButtonClicked) { TranslationKey = "gui.toTitle" }.ApplyModernStyle(false));
		}

		private void ReconnectButtonClicked()
		{
			GuiManager?.HideDialog(this);

			if (_game.ServerTypeManager.TryGet(_connectionInfo.ServerType, out var serverTypeImplementation))
			{
				_game.ConnectToServer(
					serverTypeImplementation,
					new ServerConnectionDetails(_connectionInfo.EndPoint, _connectionInfo.Hostname),
					_connectionInfo.Profile);
			}
		}

		private void MenuButtonClicked()
		{
			_game.GameStateManager.SetActiveState<TitleState>(false, false);
			GuiManager?.HideDialog(this);

			_game.IsMouseVisible = true;
		}

		/// <inheritdoc />
		public override void OnShow()
		{
			_game.IsMouseVisible = true;
			//_activeDialog = this;
			base.OnShow();
		}

		/// <inheritdoc />
		public override void OnClose()
		{
			// _activeDialog = null;
			OnDialogClosed?.Invoke(this, EventArgs.Empty);
			base.OnClose();
		}

		public void UpdateText(string reason, bool isTranslation = false)
		{
			if (isTranslation)
			{
				DisconnectedTextElement.TranslationKey = reason;
			}
			else
			{
				DisconnectedTextElement.Text = reason;
			}
		}

		public static void Show(Alex game,
			string reason,
			bool isTranslation = false,
			ConnectionInfo connectionInfo = null)
		{
			game.IsMouseVisible = true;
			var activeScreen = game.GuiManager.ActiveDialog as DisconnectedDialog;

			if (activeScreen == null)
			{
				//activeScreen = new DisconnectedDialog(game);
				activeScreen = game.GuiManager.CreateDialog<DisconnectedDialog>();
			}

			if (activeScreen != null)
			{
				activeScreen.ConnectionInfo = connectionInfo;
				activeScreen.UpdateText(reason, isTranslation);
			}
			//game.GuiManager.ShowDialog(activeScreen);
		}
	}
}