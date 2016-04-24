using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Alex.Utils;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex
{
    public static class ResManager
    {
        private static readonly Dictionary<string, Vector2> Atlaslocations = new Dictionary<string, Vector2>();
        private static Texture2D _atlas;
        public static Vector2 AtlasSize { get; private set; }

        public static float InHeigth
        {
            get { return AtlasSize.Y/16; }
        }

        public static float InWidth
        {
            get { return AtlasSize.X / 16; }
        }

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

            Texture2D texture;
            using (var memoryStream = new MemoryStream(bufferSize))
            {
                image.Save(memoryStream, ImageFormat.Png);
                texture = Texture2D.FromStream(Game.GraphicsDevice, memoryStream);
            }
            image.Dispose();
            return texture;
        }


        public static void CheckResources()
        {
            if (Directory.Exists("assets")) return;
            Directory.CreateDirectory("assets");

            var tempFileName = Path.GetTempFileName();

            var sw = new Stopwatch();
            Logging.Info("Downloading resources...");
            sw.Start();
            var client = new WebClient();
            client.DownloadFile(string.Format("https://s3.amazonaws.com/Minecraft.Download/versions/{0}/{0}.jar", "1.9.2"), tempFileName);
            sw.Stop();
            Logging.Info("Downloading took: " + Math.Round((double)(sw.ElapsedMilliseconds / 1000), 2) +
                         " seconds to finish");
            Logging.Info("Extracting resources...");
            var zf = ZipFile.Read(tempFileName);
            foreach (var e in zf)
            {
                if (e.FileName.EndsWith(".class")) continue;
                e.Extract("./", ExtractExistingFileAction.OverwriteSilently);
            }
            Logging.Info("Done!");

            //if (File.Exists("resources.temp")) File.Delete("resources.temp");
        }

        public static void InitAtlas()
        {
            var a = new AtlasGenerator();
            _atlas = a.GenerateAtlas("assets\\minecraft\\textures\\blocks");
            AtlasSize = new Vector2(_atlas.Width, _atlas.Height);
        }
    }
}
