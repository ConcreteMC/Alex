using System.Globalization;
using System.Runtime.Serialization;

namespace Alex.Common.Data.Options
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
        
        [DataMember]
        public OptionsProperty<bool> UseChunkCache { get; set; }

        [DataMember]
        public OptionsProperty<bool> LoadServerResources { get; set; }
        
        public MiscelaneousOptions()
        {
            ServerSideLighting = new OptionsProperty<bool>(false);
            Language = new OptionsProperty<string>(CultureInfo.InstalledUICulture.Name);
            MeshInRam = new OptionsProperty<bool>(true);
            ObjectPools = new OptionsProperty<bool>(true);
            UseChunkCache = new OptionsProperty<bool>(false);
            LoadServerResources = new OptionsProperty<bool>(false);
        }
    }
}