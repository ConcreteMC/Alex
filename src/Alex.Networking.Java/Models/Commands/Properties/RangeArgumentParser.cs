using System.Text;

namespace Alex.Networking.Java.Models.Commands.Properties
{
	public class RangeArgumentParser<T> : ArgumentParser
	{
		public byte Flags { get; set; }
		public T? Min { get; set; }
		public T? Max { get; set; }

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
		public RangeArgumentParser(string name) : base(name) { }
	}
}