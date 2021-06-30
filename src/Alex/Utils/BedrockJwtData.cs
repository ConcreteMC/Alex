using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiNET.Net;
using Newtonsoft.Json;

namespace Alex.Utils
{
    public class BedrockJwtData
    {
        public bool ThirdPartyNameOnly { get; set; } = false;
        public string ThirdPartyName { get; set; }


        public int ClientRandomId { get; set; }
        public string ServerAddress { get; set; }
        public string LanguageCode { get; set; }

        public string PlatformOfflineId { get; set; } = "";
        public string PlatformOnlineId { get; set; } = "";

        [JsonProperty("SelfSignedId")] public string SelfSignedID { get; set; } = Guid.NewGuid().ToString();
        public string DeviceId { get; set; } = Guid.NewGuid().ToString();
        public string DeviceModel { get; set; } = "(Standard system devices) System devices";
        public int DeviceOS { get; set; } = 1;

        public int UIProfile { get; set; } = 0;
        public int GuiScale { get; set; } = 0;

        public int DefaultInputMode { get; set; } = 1;
        public int CurrentInputMode { get; set; } = 1;

        [JsonProperty("PlayFabId")]
        public string PlayFabID { get; set; } = "";

        public string GameVersion { get; set; } = McpeProtocolInfo.GameVersion;
        public string SkinResourcePatch { get; set; } = "";

        public string SkinId { get; set; }
        public string SkinData { get; set; }
        public int SkinImageHeight { get; set; }
        public int SkinImageWidth { get; set; }

        public string CapeId { get; set; } = "";
        public int CapeImageHeight { get; set; }
        public int CapeImageWidth { get; set; }
        public string CapeData { get; set; } = "";

        public string SkinGeometryData { get; set; } = "";

        public string SkinAnimationData { get; set; } = "";
        public SkinAnimation[] AnimatedImageData { get; set; } = new SkinAnimation[0];

        public bool PremiumSkin { get; set; }
        public bool PersonaSkin { get; set; }
        public bool CapeOnClassicSkin { get; set; }

        public string SkinColor { get; set; } = "#0";
        public string ArmSize   { get; set; } = "slim";

        [JsonProperty("PersonaPieces")]
        public List<PersonaPiece> PersonaPieces    { get; set; } = new List<PersonaPiece>();
        
        [JsonProperty("PieceTintColors")]
        public List<PieceTint>    PieceTintColours { get; set; } = new List<PieceTint>();

        public BedrockJwtData(MiNET.Utils.Skins.Skin skin)
        {
            SkinResourcePatch = skin.ResourcePatch ?? Convert.ToBase64String(Encoding.Default.GetBytes(MiNET.Utils.Skins.Skin.ToJson(skin.SkinResourcePatch)));
            
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

            SkinColor = skin.SkinColor;
            ArmSize = skin.ArmSize;
        }

        public BedrockJwtData()
        {
            
        }
    }
}