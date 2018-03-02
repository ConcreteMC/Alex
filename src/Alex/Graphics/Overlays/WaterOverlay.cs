using System.Drawing;
using Alex.Gamestates;
using Alex.Utils;
using log4net;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Overlays
{
    public class WaterOverlay : IOverlay
    {
	    private static ILog Log = LogManager.GetLogger(typeof(WaterOverlay));

	    private Texture2D _waterOverlayTexture = null;
	    public void Load(GraphicsDevice graphics, ResourceManager resources)
	    {
		    if (resources.ResourcePack.TryGetBitmap("assets/minecraft/textures/misc/underwater.png", out Bitmap bmp))
		    {
			    _waterOverlayTexture = TextureUtils.BitmapToTexture2D(graphics, bmp);
			}
		    else
		    {
				Log.Warn($"Could not load underwater overlay!");
		    }
	    }

	    public void Render(RenderArgs args)
	    {
		    if (_waterOverlayTexture == null) return;

		    args.SpriteBatch.Begin();

		    //Start draw background
		    var retval = new Microsoft.Xna.Framework.Rectangle(
			    args.SpriteBatch.GraphicsDevice.Viewport.X,
			    args.SpriteBatch.GraphicsDevice.Viewport.Y,
			    args.SpriteBatch.GraphicsDevice.Viewport.Width,
			    args.SpriteBatch.GraphicsDevice.Viewport.Height);
		    args.SpriteBatch.Draw(_waterOverlayTexture, retval, Microsoft.Xna.Framework.Color.White);
		    //End draw backgroun

		    args.SpriteBatch.End();
		}
    }
}
