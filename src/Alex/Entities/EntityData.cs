using Newtonsoft.Json;

namespace Alex.Entities
{
	using J = Newtonsoft.Json.JsonPropertyAttribute;

    public class EntityData
    {
	    [J("id")] public long Id { get; set; }
	    [J("internalId")] public long InternalId { get; set; }
	    [J("name")] public string Name { get; set; }
	    [J("displayName")] public string DisplayName { get; set; }
	    [J("width")] public long Width { get; set; }
	    [J("height")] public long Height { get; set; }
	    [J("type")] public string Type { get; set; }
	    [J("category")] public string Category { get; set; }
	    
	    [JsonIgnore]
	    public string OriginalName { get; set; }
    }
}