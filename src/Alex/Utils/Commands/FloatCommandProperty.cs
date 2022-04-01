using Alex.Interfaces;

namespace Alex.Utils.Commands
{
	public class FloatCommandProperty : CommandProperty
	{
		public float MinValue { get; set; }
		public float MaxValue { get; set; }

		/// <inheritdoc />
		public FloatCommandProperty(string name, bool required = true) : base(name, required, "float") { }

		/// <inheritdoc />
		public override bool TryParse(ISeekableTextReader reader)
		{
			if (reader.ReadSingleWord(out string result) > 0)
			{
				if (float.TryParse(result, out float val))
				{
					return true;
				}
			}

			return false;
		}
	}
}