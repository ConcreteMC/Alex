using Newtonsoft.Json;

namespace Alex.Utils.Skins
{
	public class PersonaPiece
	{
		[JsonProperty("IsDefault")]
		public bool Default { get; set; } = true; //indicates if it is a persona skin part which is the default for skins
		
		[JsonProperty("PackId")]
		public string PackId { get; set; } = ""; //which is a UUID of the pack the piece belongs to
		
		[JsonProperty("PieceId")]
		public string PieceId { get; set; } = ""; //a UUID unique to the piece
		
		[JsonProperty("PieceType")]
		public string PieceType { get; set; } = "persona_body"; 
		// PieceType holds the type of the piece. Several types I was able to find immediately are listed below.
		// - persona_skeleton
		// - persona_body
		// - persona_skin
		// - persona_bottom
		// - persona_feet
		// - persona_top
		// - persona_mouth
		// - persona_hair
		// - persona_eyes
		// - persona_facial_hair
		
		[JsonProperty("ProductId")]
		public string ProductId { get; set; } = ""; //a UUID identifying the piece for purchases  (empty for free pieces)
	}
}