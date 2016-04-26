using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Utils
{
    public class AtlasGenerator
    {
        private void CopyRegionIntoImage(Bitmap srcBitmap, System.Drawing.Rectangle srcRegion, ref Bitmap destBitmap,
            System.Drawing.Rectangle destRegion)
        {
            using (var grD = System.Drawing.Graphics.FromImage(destBitmap))
            {
                grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
            }
        }

        public Texture2D GenerateAtlas(string folder)
        {
            var bitmaps = Directory.GetFiles(folder)
                .Where(file => file.EndsWith(".png") 
                && !file.Contains("lava") 
                && !file.Contains("water_still")
                && !file.Contains("water_flow")
                && !file.Contains("fire") 
                && !file.Contains("portal")).ToDictionary(Path.GetFileNameWithoutExtension, file => new Bitmap(file));

            var height = 0;
            var width = 256;

            var count = 0;
            foreach (var bm in bitmaps.Values)
            {
                if (count == 16)
                {
                    height += bm.Height;
                    count = 0;
                }
                count++;
            }
            var bitmap = new Bitmap(width, height);

            var xi = 0;
            var yi = 0;
            count = 0;

            foreach (var bm in bitmaps)
            {
                if (count == 16)
                {
                    yi += 16;
                    xi = 0;
                    count = 0;
                }
                count++;

                var r = new System.Drawing.Rectangle(0, 0, 16, 16);
                var destination = new System.Drawing.Rectangle(xi, yi, 16, 16);

                CopyRegionIntoImage(bm.Value, r, ref bitmap, destination);
                //ResManager.AddTextureLocation(bm.Key, new Vector2((float)Math.Round((double)xi / 16), (float)Math.Round((double)yi / 16)));
                ResManager.AddTextureLocation(bm.Key, new Vector2(xi, yi));
                xi += 16;
            }
            bitmap.Save("assets\\terrain.png", ImageFormat.Png);

            return Texture2D.FromStream(Game.GraphicsDevice, File.OpenRead("assets\\terrain.png"));
        }
    }
}
