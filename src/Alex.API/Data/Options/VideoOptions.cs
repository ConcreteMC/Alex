using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Alex.API.Data.Options
{
    [DataContract]
    public class VideoOptions : OptionsBase
    {
        [DataMember]
        public OptionsProperty<int> RenderDistance { get; }
        
        [DataMember]
        public OptionsProperty<bool> UseVsync { get; }
        
        [DataMember]
        public OptionsProperty<bool> Fullscreen { get; }
        
        [DataMember]
        public OptionsProperty<int> GuiScale { get; }
        
        [DataMember]
        public OptionsProperty<int> ChunkThreads { get; }

        [DataMember]
        public OptionsProperty<int> MaxFramerate { get; }
        
        public VideoOptions()
        {
            RenderDistance = DefineRangedProperty(6, 2, 32);
            UseVsync = DefineProperty(true);
            Fullscreen = DefineProperty(false);
            GuiScale = DefineRangedProperty(0, 0, 3);
            ChunkThreads = DefineRangedProperty(Environment.ProcessorCount / 2, 1, Environment.ProcessorCount);
            MaxFramerate = DefineRangedProperty(60, 1, 999);
        }
    }
}
