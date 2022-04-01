using System.Text;
using Alex.Interfaces;

namespace Alex.Networking.Java.Commands.Parsers
{
	public abstract class RangeArgumentParser<T> : ArgumentParser
	{
		public byte Flags { get; set; }
		public T Min { get; set; }
		public T Max { get; set; }

		/// <inheritdoc />
		public override bool TryParse(ISeekableTextReader input)
		{
			if (input.ReadSingleWord(out var textInput) >= 0 && TryParse(textInput, out T value))
				return true;

			return false;
		}

		protected abstract bool TryParse(string input, out T value);

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

			sb.AppendFormat("{0}:{1}", Name, typeof(T).Name);

			if (Parent.IsExecutable)
			{
				sb.Append(']');
			}
			else
			{
				sb.Append('>');
			}

			return base.ToString();
		}

		/// <inheritdoc />
		protected RangeArgumentParser(string name) : base(name) { }
	}
}