using System;
using System.Runtime.Serialization;

namespace Alex.Common.Data.Options
{
    [DataContract]
    public class VideoOptions : OptionsBase
    {
        [DataMember]
        public OptionsProperty<int> RenderDistance { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> UseVsync { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> LimitFramerate { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> Fullscreen { get; set; }
        
        [DataMember]
        public OptionsProperty<int> GuiScale { get; set; }

        [DataMember]
        public OptionsProperty<int> MaxFramerate { get; set; }
        
        [DataMember]
        public OptionsProperty<int> Brightness { get; set; }

        [DataMember]
        public OptionsProperty<bool> Skybox { get; set; }
        
        [DataMember]
        public OptionsProperty<int> Antialiasing { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> CustomSkins { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> EntityCulling { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> ClientSideLighting { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> SmoothLighting { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> FancyGraphics { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> Particles { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> Fog { get; set; }
        
        [DataMember]
        public OptionsProperty<int> EntityRenderDistance { get; set; }
        
        public VideoOptions()
        {
            RenderDistance = DefineRangedProperty(6, 2, 32);
            UseVsync = DefineProperty(true);
            Fullscreen = DefineProperty(false);
            GuiScale = DefineRangedProperty(1, 0, 3);
            MaxFramerate = DefineRangedProperty(60, 1, 999);
            Brightness = DefineRangedProperty(50, 0, 100);

            Antialiasing = DefineRangedProperty(8, 0, 16);
            
            LimitFramerate = DefineProperty(false);
            Skybox = DefineProperty(true);

            CustomSkins = DefineProperty(true);
            ClientSideLighting = DefineProperty(true);

            SmoothLighting = DefineProperty(true);
            FancyGraphics = DefineProperty(true);
            Particles = DefineProperty(true);

            EntityCulling = DefineProperty(false);
            Fog = DefineProperty(true);

            EntityRenderDistance = DefineRangedProperty(6, 2, 32);
        }
    }
}
