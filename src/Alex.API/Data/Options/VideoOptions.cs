using System;
using System.Runtime.Serialization;

namespace Alex.API.Data.Options
{
    [DataContract]
    public class VideoOptions : OptionsBase
    {
        [DataMember]
        public OptionsProperty<int> RenderDistance { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> UseVsync { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> Fullscreen { get; set; }
        
        [DataMember]
        public OptionsProperty<int> GuiScale { get; set; }
        
        [DataMember]
        public OptionsProperty<int> ChunkThreads { get; set; }

        [DataMember]
        public OptionsProperty<int> MaxFramerate { get; set; }
        
        [DataMember]
        public OptionsProperty<int> Brightness { get; set; }
        
        public VideoOptions()
        {
            RenderDistance = DefineRangedProperty(6, 2, 32);
            UseVsync = DefineProperty(true);
            Fullscreen = DefineProperty(false);
            GuiScale = DefineRangedProperty(1, 0, 3);
            ChunkThreads = DefineRangedProperty(Environment.ProcessorCount / 2, 1, Environment.ProcessorCount);
            MaxFramerate = DefineRangedProperty(60, 1, 999);
            Brightness = DefineRangedProperty(50, 0, 100);
        }
    }
}
