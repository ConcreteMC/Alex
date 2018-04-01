using Alex.ResourcePackLib.Json.Models.Blocks;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.BlockStates
{
	public class BlockStateModel
	{
		/// <summary>
		/// Contains the properties of a model, if more than one model is used for the same variant. 
		/// All specified models alternate in the game.
		/// </summary>
		[JsonProperty("model")]
		public string ModelName { get; set; }

		[JsonIgnore]
		public BlockModel Model { get; set; }

		/// <summary>
		/// Rotation of the model on the x-axis in increments of 90 degrees.
		/// </summary>
		public int X { get; set; } = 0;

		/// <summary>
		/// Rotation of the model on the y-axis in increments of 90 degrees.
		/// </summary>
		public int Y { get; set; } = 0;

		/// <summary>
		/// Can be true or false (default). Locks the rotation of the texture of a block, if set to true. 
		/// This way the texture will not rotate with the block when using the x and y-tags above.
		/// </summary>
		public bool Uvlock { get; set; } = false;

		/// <summary>
		/// Sets the probability of the model for being used in the game, defaults to 1 (=100%). 
		/// If more than one model is used for the same variant, the probability will be calculated by dividing the individual model’s weight by the sum of the weights of all models. 
		/// (For example, if three models are used with weights 1, 1, and 2, then their combined weight would be 4 (1+1+2). The probability of each model being used would then be determined by dividing each weight by 4: 1/4, 1/4 and 2/4, or 25%, 25% and 50%, respectively.)
		/// </summary>
		public int Weight { get; set; } = 1;
	}
}