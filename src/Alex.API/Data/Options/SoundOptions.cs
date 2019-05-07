using System.Runtime.Serialization;

namespace Alex.API.Data.Options
{
    [DataContract]
    public class SoundOptions : OptionsBase
    {
        [DataMember]
        public OptionsProperty<float> GlobalVolume { get; }

        [DataMember]
        public OptionsProperty<float> MusicVolume { get; }

        [DataMember]
        public OptionsProperty<float> SoundEffectsVolume { get; }


        public SoundOptions()
        {
            GlobalVolume        = DefineRangedProperty(1.0f, 0.0f, 1.0f);
            MusicVolume         = DefineRangedProperty(1.0f, 0.0f, 1.0f);
            SoundEffectsVolume  = DefineRangedProperty(1.0f, 0.0f, 1.0f);
        }

    }
}