using Alex.API.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex.GameStates.Playing
{
	public class InGameMenuStateOld : GameState
	{
		public InGameMenuStateOld(Alex alex, KeyboardState state) : base(alex)
		{
			PreviousKeyboardState = state;
		}

		protected override void OnLoad(IRenderArgs args)
		{


			//Gui.ClassName = "TitleScreenRoot";

			//var menuWrapper = new UiPanel()
			//{
			//	ClassName = "TitleScreenMenuPanel"
			//};
			//var stackMenu = new UiMenu()
			//{
			//	ClassName = "TitleScreenMenu"
			//};

			//stackMenu.AddMenuItem("Disconnect", DisconnectButtonOnOnButtonClick);
			//stackMenu.AddMenuItem("Options", () => { Alex.GameStateManager.SetActiveState("options"); });
			//stackMenu.AddMenuItem("Return to game", ReturnButtonOnOnButtonClick);

			//menuWrapper.AddChild(stackMenu);

			//Gui.AddChild(menuWrapper);

			//var logo = new UiElement()
			//{
			//	ClassName = "TitleScreenLogo",
			//};
			//Gui.AddChild(logo);

			Alex.IsMouseVisible = true;
		}

		private void ReturnButtonOnOnButtonClick()
		{
			Alex.IsMouseVisible = false;
			Alex.GameStateManager.Back();
			Alex.GameStateManager.RemoveState("ingamemenu");
		}

		private void DisconnectButtonOnOnButtonClick()
		{
			Alex.GameStateManager.SetActiveState("title");

			Alex.GameStateManager.RemoveState("serverMenu");
			Alex.GameStateManager.RemoveState("play");
		}

		protected override void OnDraw(IRenderArgs args)
		{
			ParentState.Draw(args);

			Viewport viewPort = Viewport;
			SpriteBatch sb = args.SpriteBatch;

			sb.Begin();

			sb.FillRectangle(new Rectangle(0, 0, viewPort.Width, viewPort.Height), new Color(Color.Black, 0.5f));

			sb.End();
		}

		private KeyboardState PreviousKeyboardState { get; set; }
		protected override void OnUpdate(GameTime gameTime)
		{
			if (Alex.IsActive)
			{
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
