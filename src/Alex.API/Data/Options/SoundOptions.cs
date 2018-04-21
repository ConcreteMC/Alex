namespace Alex.API.Data.Options
{
    public class SoundOptions : OptionsBase
    {
        public OptionsProperty<float> GlobalVolume { get; }
        public OptionsProperty<float> MusicVolume { get; }
        public OptionsProperty<float> SoundEffectsVolume { get; }


        public SoundOptions()
        {
            GlobalVolume        = DefineRangedProperty(1.0f, 0.0f, 1.0f);
            MusicVolume         = DefineRangedProperty(1.0f, 0.0f, 1.0f);
            SoundEffectsVolume  = DefineRangedProperty(1.0f, 0.0f, 1.0f);
        }

    }
}