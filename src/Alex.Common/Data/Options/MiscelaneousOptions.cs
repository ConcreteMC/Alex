using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Alex.Common.Data.Options
{
    [DataContract]
    public class MiscelaneousOptions : OptionsBase
    {
                
        [DataMember]
        public OptionsProperty<int> ChunkThreads { get; set; }

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
        
        [DataMember]
        public OptionsProperty<bool> ShowNetworkInfoByDefault { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> Minimap { get; set; }
        
        [DataMember]
        public OptionsProperty<double> MinimapSize { get; set; }
        
        public MiscelaneousOptions()
        {
            ChunkThreads = DefineRangedProperty(Environment.ProcessorCount / 2, 1, Environment.ProcessorCount);
            Language = DefineProperty(CultureInfo.InstalledUICulture.Name);
            MeshInRam = DefineProperty(true);
            ObjectPools = DefineProperty(true);
            UseChunkCache = DefineProperty(false);
            LoadServerResources = DefineProperty(false);
            ShowNetworkInfoByDefault = DefineProperty(false);
            Minimap = DefineProperty(false);
            MinimapSize = DefineRangedProperty(1d, 0.125d, 2d);
        }
    }
}