using System.Runtime.Serialization;

namespace Alex.API.Data.Options
{
    [DataContract]
    public class SoundOptions : OptionsBase
    {
        [DataMember]
        public OptionsProperty<float> GlobalVolume { get; set; }

        [DataMember]
        public OptionsProperty<float> MusicVolume { get; set; }

        [DataMember]
        public OptionsProperty<float> SoundEffectsVolume { get; set; }


        public SoundOptions()
        {
            GlobalVolume        = DefineRangedProperty(1.0f, 0.0f, 1.0f);
            MusicVolume         = DefineRangedProperty(1.0f, 0.0f, 1.0f);
            SoundEffectsVolume  = DefineRangedProperty(1.0f, 0.0f, 1.0f);
        }

    }
}