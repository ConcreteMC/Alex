using System.Runtime.Serialization;

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

        public VideoOptions()
        {
            RenderDistance = DefineRangedProperty(16, 2, 32);
            UseVsync = DefineProperty(true);
            Fullscreen = DefineProperty(false);
            GuiScale = DefineRangedProperty(0, 0, 3);
        }
    }
}
