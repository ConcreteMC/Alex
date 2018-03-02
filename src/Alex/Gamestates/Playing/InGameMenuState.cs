using Alex.Rendering.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex.Gamestates.Playing
{
	public class InGameMenuState : Gamestate
	{
		private Alex Alex { get; }
		private PlayingState State { get; }
		public InGameMenuState(Alex alex, PlayingState playingState, KeyboardState state) : base(alex)
		{
			Alex = alex;
			State = playingState;
			PreviousKeyboardState = state;

			Button disconnectButton = new Button("Disconnect");
			disconnectButton.OnButtonClick += DisconnectButtonOnOnButtonClick;

			Button returnButton = new Button("Return to game");
			returnButton.OnButtonClick += ReturnButtonOnOnButtonClick;

			Controls.Add("disconnectBtn", disconnectButton);
			Controls.Add("returnBtn", returnButton);
			Controls.Add("info", new Info());

			Alex.IsMouseVisible = true;
		}

		private void ReturnButtonOnOnButtonClick()
		{
			Alex.IsMouseVisible = false;
			Alex.GamestateManager.SetActiveState(State);
		}

		private void DisconnectButtonOnOnButtonClick()
		{
			// State.Disconnect();
			Alex.GamestateManager.SetActiveState("menu");
			Alex.GamestateManager.RemoveState("serverMenu");
			Alex.GamestateManager.RemoveState("play");
		}

		public override void Render3D(RenderArgs args)
		{
			State.Render3D(args);
		}

		public override void Render2D(RenderArgs args)
		{
			Viewport viewPort = Viewport;
			SpriteBatch sb = args.SpriteBatch;

			sb.Begin();

			sb.FillRectangle(new Rectangle(0, 0, viewPort.Width, viewPort.Height), new Color(Color.Black, 0.5f));

			sb.End();
		}

		private KeyboardState PreviousKeyboardState { get; set; }
		public override void OnUpdate(GameTime gameTime)
		{
			Controls["returnBtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y - 30);
			Controls["disconnectBtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20);

			if (Alex.IsActive)
			{
				//State.SendPositionUpdate(gameTime);

				KeyboardState currentKeyboardState = Keyboard.GetState();
				if (currentKeyboardState != PreviousKeyboardState)
				{
					if (currentKeyboardState.IsKeyDown(KeyBinds.Menu))
					{
						ReturnButtonOnOnButtonClick();
					}
				}
				PreviousKeyboardState = currentKeyboardState;
			}
		}
	}
}
