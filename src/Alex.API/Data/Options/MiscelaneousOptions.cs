using System.Runtime.Serialization;

namespace Alex.API.Data.Options
{
    [DataContract]
    public class MiscelaneousOptions : OptionsBase
    {
        [DataMember]
        public OptionsProperty<bool> ServerSideLighting { get; set; }
        
        [DataMember]
        public OptionsProperty<string> Language { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> MeshInRam { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> ObjectPools { get; set; }

        public MiscelaneousOptions()
        {
            ServerSideLighting = new OptionsProperty<bool>(false);
            Language = new OptionsProperty<string>("en_us");
            MeshInRam = new OptionsProperty<bool>(true);
            ObjectPools = new OptionsProperty<bool>(true);
        }
    }
}