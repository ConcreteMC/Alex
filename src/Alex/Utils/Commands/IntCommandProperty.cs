namespace Alex.Utils.Commands
{
	public class IntCommandProperty : CommandProperty
	{
		public int MinValue { get; set; }
		public int MaxValue { get; set; }
		/// <inheritdoc />
		public IntCommandProperty(string name, bool required = true) : base(name, required, "integer")
		{
			
		}
		
		/// <inheritdoc />
		public override bool TryParse(SeekableTextReader reader)
		{
			if (reader.ReadSingleWord(out string result) > 0)
			{
				if (int.TryParse(result, out int val))
				{
					return true;
				}
			}

			return false;
		}
	}
}