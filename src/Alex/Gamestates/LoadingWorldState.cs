using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
    public class LoadingWorldState : Gamestate
    {
		private Texture2D Background { get; }
	    public LoadingWorldState(GraphicsDevice graphics, Texture2D background) : base(graphics)
	    {
		    Background = background;
	    }

	    private int Progress { get; set; } = 0;
	    private bool Generating { get; set; } = false;
	    public void UpdateProgress(int percentage, bool generatingVertices)
	    {
		    Progress = percentage;
		    Generating = generatingVertices;
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
		    args.SpriteBatch.Draw(Background, retval, Color.White);
		    //End draw backgroun

		    string displayString = Generating ? $"Initializing world: {Progress}%" : $"Loading world: {Progress}%";
		    var size = Alex.Font.MeasureString(displayString);
			args.SpriteBatch.DrawString(Alex.Font, displayString, CenterScreen - (size / 2), Color.White);

		    args.SpriteBatch.End();
		}
    }
}
