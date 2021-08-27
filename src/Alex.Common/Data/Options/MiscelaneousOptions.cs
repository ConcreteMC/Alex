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
        public OptionsProperty<double> AntiLagModifier { get; set; }
        
        [DataMember]
        public OptionsProperty<bool> SkipFrames { get; set; }

        public MiscelaneousOptions()
        {
            ChunkThreads = DefineRangedProperty(Environment.ProcessorCount / 2, 1, Environment.ProcessorCount);
            Language = DefineProperty(CultureInfo.InstalledUICulture.Name);
            MeshInRam = DefineProperty(true);
            ObjectPools = DefineProperty(true);
            UseChunkCache = DefineProperty(false);
            LoadServerResources = DefineProperty(false);
            ShowNetworkInfoByDefault = DefineProperty(false);
            AntiLagModifier = DefineRangedProperty(0.75d, 0d, 1d);
            SkipFrames = DefineProperty(true);
        }
    }
}