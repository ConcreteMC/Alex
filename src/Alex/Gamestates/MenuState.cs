using System;
using System.IO;
using Alex.API.World;
using Alex.Gamestates.Playing;
using Alex.Rendering.UI;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Generators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Worlds;
using AnvilWorldProvider = Alex.Worlds.AnvilWorldProvider;

namespace Alex.Gamestates
{
	public class MenuState : GameState
	{
		public MenuState(Alex alex) : base(alex)
		{
			
		}

		private Texture2D BackGround { get; set; }

		protected override void OnLoad(RenderArgs args)
		{
			BackGround = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.mcbg);

			//Alex.ShowMouse();
			Alex.IsMouseVisible = true;

			Button mpbtn = new Button("Multiplayer")
			{
				Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20),
			};
			mpbtn.OnButtonClick += Mpbtn_OnButtonClick;

			Controls.Add("mpbtn", mpbtn);

			Button button = new Button("Debug world")
			{
				Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 70),
			};
			button.OnButtonClick += OnDebugWorldClick;

			Controls.Add("testbtn", button);

			Button opton = new Button("Settings")
			{
				Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 240),
			};
			opton.OnButtonClick += Opton_OnButtonClick;

			Controls.Add("optbtn", opton);

			Button logoutbtn = new Button("Logout")
			{
				Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 120),
			};
			logoutbtn.OnButtonClick += Logoutbtn_OnButtonClick;

			Controls.Add("logoutbtn", logoutbtn);

			/*             Controls.Add("input", new InputField()
						 {
							 Location = new Vector2(5, 5)
						 });

						 Controls.Add("track", new TrackBar()
						 {
							 Location = new Vector2(5, 55),
							 Text = "Change Me",
							 MaxValue = 12,
							 MinValue = 2,
							 Value = 6
						 });
						 */

			Controls.Add("logo", new Logo());
			Controls.Add("info", new Info());
		}

		private void OnDebugWorldClick()
		{
			Alex.IsMouseVisible = false;

			IWorldGenerator generator;
			if (Alex.GameSettings.UseBuiltinGenerator || (string.IsNullOrWhiteSpace(Alex.GameSettings.Anvil) || !File.Exists(Path.Combine(Alex.GameSettings.Anvil, "level.dat"))))
			{
				generator = new OverworldGenerator();
			}
			else
			{
				generator = new AnvilWorldProvider(Alex.GameSettings.Anvil)
				{
					MissingChunkProvider = new VoidWorldGenerator()
				};
			}

			generator.Initialize();

			LoadWorld(new SPWorldProvider(Alex, generator));
		}

		private void LoadWorld(WorldProvider worldProvider)
		{
			LoadingWorldState loadingScreen = new LoadingWorldState(Alex, BackGround);
			Alex.GameStateManager.AddState("loading", loadingScreen);
			Alex.GameStateManager.SetActiveState("loading");

			worldProvider.Load(loadingScreen.UpdateProgress).ContinueWith(task =>
			{
				PlayingState playState = new PlayingState(Alex, Graphics, worldProvider);
				Alex.GameStateManager.AddState("play", playState);
				Alex.GameStateManager.SetActiveState("play");

				Alex.GameStateManager.RemoveState("loading");
			});
		}

		private void Logoutbtn_OnButtonClick()
		{
			Alex.GameStateManager.SetActiveState("login");
		}

		private void Opton_OnButtonClick()
		{
			//Todo
		}

		private void Mpbtn_OnButtonClick()
		{
			Alex.GameStateManager.AddState("serverMenu", new ServerState(Alex));
			Alex.GameStateManager.SetActiveState("serverMenu");
		}

		protected override void OnUnload()
		{
			//Alex.HideMouse();
			Alex.IsMouseVisible = false;
		}

		protected override void OnDraw2D(RenderArgs args)
		{
			args.SpriteBatch.Begin();

			//Start draw background
			var retval = new Rectangle(
				args.SpriteBatch.GraphicsDevice.Viewport.X,
				args.SpriteBatch.GraphicsDevice.Viewport.Y,
				args.SpriteBatch.GraphicsDevice.Viewport.Width,
				args.SpriteBatch.GraphicsDevice.Viewport.Height);
			args.SpriteBatch.Draw(BackGround, retval, Color.White);
			//End draw backgroun

			args.SpriteBatch.End();
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			Controls["mpbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y - 30);

			Controls["testbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20);

			Controls["optbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 70);

			Controls["logoutbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 120);

			//TrackBar track = (TrackBar) Controls["track"];
			//track.Text = "Render distance: " + track.Value;
		}
	}
}
