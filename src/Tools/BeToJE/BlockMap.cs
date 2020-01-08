using Newtonsoft.Json;

namespace BeToJE
{
    public partial class BlockMap
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("States")]
        public State[] States { get; set; }

        [JsonProperty("Data")]
        public long Data { get; set; }

        [JsonProperty("RuntimeId")]
        public long RuntimeId { get; set; }
    }

    public partial class State
    {
        [JsonProperty("Type")]
        public long Type { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}