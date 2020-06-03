using System.Runtime.Serialization;

namespace Alex.API.Data.Options
{
    [DataContract]
    public class AlexOptions : OptionsBase
    {
        [DataMember]
        public OptionsProperty<int> FieldOfVision { get; set; }
        
        [DataMember]
        public OptionsProperty<int> MouseSensitivity { get; set; } 
        
        [DataMember]
        public VideoOptions VideoOptions { get; set; }

        [DataMember]
        public SoundOptions SoundOptions { get; set; }
        
        [DataMember]
        public ResourceOptions ResourceOptions { get; set; }
        
        [DataMember]
        public MiscelaneousOptions MiscelaneousOptions { get; set; }

        [DataMember]
        public NetworkOptions NetworkOptions { get; set; }
        
        public AlexOptions()
        {
            FieldOfVision = DefineRangedProperty(70, 30, 120);
            MouseSensitivity = DefineRangedProperty(30, 0, 60);
            
            VideoOptions = DefineBranch<VideoOptions>();
            SoundOptions = DefineBranch<SoundOptions>();
            ResourceOptions = DefineBranch<ResourceOptions>();
            MiscelaneousOptions = DefineBranch<MiscelaneousOptions>();
            NetworkOptions = DefineBranch<NetworkOptions>();
        }
    }
}
