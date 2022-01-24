using Newtonsoft.Json;

namespace Alex.Utils.Skins
{
	public class PieceTint
	{
		//holds a list of hex colours relevant to the piece
		// Some pieces have only one colour, for which the rest will be filled with #0
		//Some pieces like the eyes have different colours for the iris and the eyebrows for example

		[JsonProperty("Colors")] public string[] Colors { get; set; } = new string[4] { "#0", "#0", "#0", "#0" };

		[JsonProperty("PieceType")]
		public string
			PieceType { get; set; } //is the piece type that it refers to, which is present in the previous list
	}
}