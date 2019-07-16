using System.Runtime.Serialization;

namespace Alex.API.Data.Options
{
    [DataContract]
    public class AlexOptions : OptionsBase
    {
        [DataMember]
        public OptionsProperty<int> FieldOfVision { get; }
        
        [DataMember]
        public VideoOptions VideoOptions { get; }

        [DataMember]
        public SoundOptions SoundOptions { get; }
        
        [DataMember]
        public ResourceOptions ResourceOptions { get; }
        
        [DataMember]
        public MiscelaneousOptions MiscelaneousOptions { get; }

        public AlexOptions()
        {
            FieldOfVision = DefineRangedProperty(70, 30, 120);

            VideoOptions = DefineBranch<VideoOptions>();
            SoundOptions = DefineBranch<SoundOptions>();
            ResourceOptions = DefineBranch<ResourceOptions>();
            MiscelaneousOptions = DefineBranch<MiscelaneousOptions>();
        }
    }
}
