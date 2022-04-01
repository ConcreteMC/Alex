using System.Text;
using Alex.Interfaces;
using Alex.Networking.Java.Commands.Nodes;

namespace Alex.Networking.Java.Commands.Parsers
{
	public interface IArgumentParser
	{
		string Name { get; }

		bool TryParse(ISeekableTextReader reader);
	}

	public interface ISuggestive : IArgumentParser
	{
		bool TryParse(ISeekableTextReader reader, out string[] matches);
	}

	public abstract class ArgumentParser : IArgumentParser
	{
		public CommandNode Parent { get; set; }
		public string Name { get; set; }

		protected ArgumentParser(string name)
		{
			Name = name;
		}

		public abstract bool TryParse(ISeekableTextReader input);

		/// <inheritdoc />
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (Parent.IsExecutable)
			{
				sb.Append('[');
			}
			else
			{
				sb.Append('<');
			}

			sb.AppendFormat("{0}", Name);

			if (Parent.IsExecutable)
			{
				sb.Append(']');
			}
			else
			{
				sb.Append('>');
			}

			return sb.ToString();
		}

		public bool TryParse(ISeekableTextReader input, out string s)
		{
			var startPosition = input.Position;

			if (TryParse(input))
			{
				var endPosition = input.Position;
				var length = endPosition - startPosition;

				input.Position = startPosition;
				char[] text = new char[length];
				input.Read(text, 0, text.Length);
				s = new string(text);

				return true;
			}

			s = null;

			return false;
		}

		public bool ParseRelative(string input, out double value)
		{
			if (input[0] == '~')
			{
				input = input.Substring(1);
			}

			return double.TryParse(input, out value);
		}

		public bool ParseRelative(string input, out int value)
		{
			if (input[0] == '~')
			{
				input = input.Substring(1);
			}

			return int.TryParse(input, out value);
		}
	}
}