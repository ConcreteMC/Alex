using System;
using System.Linq;
using MiNET.Utils.Skins;
using Newtonsoft.Json;

namespace Alex.Worlds.Bedrock
{
    public class BedrockSkinData
    {
        public int ClientRandomId { get; set; }
        public string ServerAddress { get; set; }
        public string LanguageCode { get; set; }

        public string SkinResourcePatch { get; set; }

        public string SkinId;
        public string SkinData;
        public int SkinImageHeight;
        public int SkinImageWidth;

        public string CapeId;
        public int CapeImageHeight;
        public int CapeImageWidth;
        public string CapeData;

        public string SkinGeometryData;

        public string SkinAnimationData;
        public SkinAnimation[] AnimatedImageData;

        public bool PremiumSkin;
        public bool PersonaSkin;
        public bool CapeOnClassicSkin;

        public class SkinAnimation
        {
            public string Image { get; set; }
            public int ImageWidth { get; set; }
            public int ImageHeight { get; set; }
            public float FrameCount { get; set; }
            public int Type { get; set; } // description above

            public SkinAnimation(Animation animation)
            {
                Image = Convert.ToBase64String(animation.Image);
                ImageWidth = animation.ImageWidth;
                ImageHeight = animation.ImageHeight;
                FrameCount = animation.FrameCount;
                Type = animation.Type;
            }
        }

        public BedrockSkinData(Skin skin)
        {
            SkinResourcePatch = skin.ResourcePatch;
            
            SkinId = skin.SkinId;
            SkinData = Convert.ToBase64String(skin.Data);
            SkinImageHeight = skin.Height;
            SkinImageWidth = skin.Width;

            CapeId = skin.Cape.Id;
            CapeImageHeight = skin.Cape.ImageHeight;
            CapeImageWidth = skin.Cape.ImageWidth;
            CapeData = Convert.ToBase64String(skin.Cape.Data);

            SkinGeometryData = skin.GeometryData;
            SkinAnimationData = skin.AnimationData;

            AnimatedImageData = skin.Animations.Select(x => new SkinAnimation(x)).ToArray();

            PremiumSkin = skin.IsPremiumSkin;
            PersonaSkin = skin.IsPersonaSkin;
            CapeOnClassicSkin = skin.Cape.OnClassicSkin;
        }
    }
}