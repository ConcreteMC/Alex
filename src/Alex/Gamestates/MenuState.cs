using System;
using Alex.Rendering.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class MenuState : Gamestate
	{
	    private Alex Alex { get; }
	    public MenuState(Alex alex) : base(alex.GraphicsDevice)
	    {
	        Alex = alex;
	    }

		private Texture2D BackGround { get; set; }
		public override void Init(RenderArgs args)
		{
			BackGround = ResManager.ImageToTexture2D(Properties.Resources.mcbg);

			//Alex.ShowMouse();
			Alex.IsMouseVisible = true;

            Button mpbtn = new Button("Multiplayer")
            {
                Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20),
            };
            mpbtn.OnButtonClick += Mpbtn_OnButtonClick;

            Controls.Add("mpbtn", mpbtn);

           // Button button = new Button("Debug world")
          //  {
           //     Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 70),
           // };
          //  button.OnButtonClick += button_OnButtonClick;

           // Controls.Add("testbtn", button);

            Button opton = new Button("Settings")
            {
                Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 120),
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

        private void Logoutbtn_OnButtonClick()
        {
            Alex.GamestateManager.SetActiveState("login");
        }

        private void Opton_OnButtonClick()
        {
            //Todo
        }

        private void Mpbtn_OnButtonClick()
        {
            Alex.GamestateManager.AddState("serverMenu", new ServerState(Alex));
            Alex.GamestateManager.SetActiveState("serverMenu");
        }

		public override void Stop()
		{
			//Alex.HideMouse();
			Alex.IsMouseVisible = false;
		}

		public override void Render2D(RenderArgs args)
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

		public override void OnUpdate(GameTime gameTime)
        {
            Controls["mpbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y - 30);

            //    Controls["testbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 70);

            Controls["optbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20);

            Controls["logoutbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 70);

		    //TrackBar track = (TrackBar) Controls["track"];
		    //track.Text = "Render distance: " + track.Value;
        }
	}
}
