using Alex.Interfaces;

namespace Alex.Utils.Commands
{
	public class DoubleCommandProperty : CommandProperty
	{
		public double MinValue { get; set; }
		public double MaxValue { get; set; }

		/// <inheritdoc />
		public DoubleCommandProperty(string name, bool required = true) : base(name, required, "float") { }

		/// <inheritdoc />
		public override bool TryParse(ISeekableTextReader reader)
		{
			if (reader.ReadSingleWord(out string result) > 0)
			{
				if (double.TryParse(result, out double val))
				{
					return true;
				}
			}

			return false;
		}
	}
}