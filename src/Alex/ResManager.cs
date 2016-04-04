using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex
{
    public static class ResManager
    {
        private static readonly Dictionary<string, Vector2> Atlaslocations = new Dictionary<string, Vector2>();
        private static Texture2D _atlas;
        public static Vector2 AtlasSize { get; private set; }

        public static Texture2D GetAtlas()
        {
            if (_atlas != null) return _atlas;

            InitAtlas();
            return _atlas;
        }

        public static void AddTextureLocation(string texture, Vector2 location)
        {
            if (Atlaslocations.ContainsKey(texture)) return;

            Atlaslocations.Add(texture, location);
        }

        public static Vector2 GetAtlasLocation(string file)
        {
            if (Atlaslocations.Count == 0) InitAtlas();

            return Atlaslocations.ContainsKey(file) ? Atlaslocations[file] : Vector2.Zero;
        }

        public static Texture2D ImageToTexture2D(Image bmp)
        {
            var image = new Bitmap(new Bitmap(bmp));
            var bufferSize = image.Height * image.Width * 4;

            using (var memoryStream = new MemoryStream(bufferSize))
            {
                image.Save(memoryStream, ImageFormat.Png);

                var texture = Texture2D.FromStream(Game.GraphicsDevice, memoryStream);
                return texture;
            }
        }


        public static void CheckResources()
        {
            /*if (Directory.Exists("Content") && Directory.Exists("Content/assets")) return;
            Directory.CreateDirectory("Content");

            var sw = new Stopwatch();
            //Logging.Info("Downloading resources...");
            sw.Start();
            var client = new WebClient();
            client.DownloadFile(string.Format(Variables.ResourceUrl, Variables.MinecraftVersion), "resources.temp");
            sw.Stop();
            //Logging.Info("Downloading took: " + Math.Round((double)(sw.ElapsedMilliseconds / 1000), 2) +
            //             " seconds to finish");
            //Logging.Info("Extracting resources...");
            var zf = ZipFile.Read("resources.temp");
            foreach (var e in zf)
            {
                if (e.FileName.EndsWith(".class")) continue;
                e.Extract("Content/", ExtractExistingFileAction.OverwriteSilently);
            }
            //Logging.Info("Done!");

            if (File.Exists("resources.temp")) File.Delete("resources.temp");*/
        }

        public static void InitAtlas()
        {
            var a = new AtlasGenerator();
            _atlas = a.GenerateAtlas("assets\\minecraft\\textures\\blocks");
            AtlasSize = new Vector2(_atlas.Width, _atlas.Height);
        }
    }
}
