using Alex.Interfaces;

namespace Alex.Networking.Java.Commands.Parsers
{
	public class BlockPositionArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public BlockPositionArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		public override bool TryParse(ISeekableTextReader input)
		{
			if (input.ReadSingleWord(out string x) > 0 && input.ReadSingleWord(out string y) > 0
			                                           && input.ReadSingleWord(out string z) > 0)
			{
				if (ParseRelative(x, out int _) && ParseRelative(y, out int _) && ParseRelative(z, out int _))
					return true;
			}

			return false;
		}
	}

	public class ColumnPositionArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public ColumnPositionArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		public override bool TryParse(ISeekableTextReader input)
		{
			if (input.ReadSingleWord(out string x) > 0 && input.ReadSingleWord(out string y) > 0
			                                           && input.ReadSingleWord(out string z) > 0)
			{
				if (ParseRelative(x, out int _) && ParseRelative(y, out int _) && ParseRelative(z, out int _))
					return true;
			}

			return false;
		}
	}

	public class Vector3ArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public Vector3ArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		public override bool TryParse(ISeekableTextReader input)
		{
			if (input.ReadSingleWord(out string x) > 0 && input.ReadSingleWord(out string y) > 0
			                                           && input.ReadSingleWord(out string z) > 0)
			{
				if (ParseRelative(x, out double _) && ParseRelative(y, out double _) && ParseRelative(z, out double _))
					return true;
			}

			return false;
		}
	}

	public class Vector2ArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public Vector2ArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		public override bool TryParse(ISeekableTextReader input)
		{
			if (input.ReadSingleWord(out string x) > 0 && input.ReadSingleWord(out string y) > 0)
			{
				if (ParseRelative(x, out double _) && ParseRelative(y, out double _))
					return true;
			}

			return false;
		}
	}
}