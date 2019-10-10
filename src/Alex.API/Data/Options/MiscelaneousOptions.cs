using System.Runtime.Serialization;

namespace Alex.API.Data.Options
{
    [DataContract]
    public class MiscelaneousOptions : OptionsBase
    {
        [DataMember]
        public OptionsProperty<bool> ServerSideLighting { get; set; }
        
        public MiscelaneousOptions()
        {
            ServerSideLighting = new OptionsProperty<bool>(false);
        }
    }
}