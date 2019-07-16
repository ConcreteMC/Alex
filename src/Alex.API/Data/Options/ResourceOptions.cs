using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Alex.API.Data.Options
{
    [DataContract]
    public class ResourceOptions : OptionsBase
    {
        [DataMember]
        public OptionsProperty<string> PluginDirectory { get; }
        
        [DataMember]
        public OptionsProperty<string[]> LoadedResourcesPacks { get; }

        public ResourceOptions()
        {
            PluginDirectory = new OptionsProperty<string>(null);
            LoadedResourcesPacks = new OptionsProperty<string[]>(new string[0]);
        }

        private string[] Validator(string[] currentvalue, string[] newvalue)
        {
            List<string> result = new List<string>();
            foreach (var path in newvalue)
            {
                if (File.Exists(path))
                {
                    result.Add(path);
                }
            }

            return result.ToArray();
        }
    }
}