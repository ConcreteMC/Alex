using System.Runtime.Serialization;

namespace Alex.API.Data.Options
{
    [DataContract]
    public class SoundOptions : OptionsBase
    {
        [DataMember]
        public OptionsProperty<double> GlobalVolume { get; set; }

        [DataMember]
        public OptionsProperty<double> MusicVolume { get; set; }

        [DataMember]
        public OptionsProperty<double> SoundEffectsVolume { get; set; }


        public SoundOptions()
        {
            GlobalVolume        = DefineRangedProperty(1.0, 0.0, 1.0);
            MusicVolume         = DefineRangedProperty(1.0, 0.0, 1.0);
            SoundEffectsVolume  = DefineRangedProperty(1.0, 0.0, 1.0);
        }

    }
}