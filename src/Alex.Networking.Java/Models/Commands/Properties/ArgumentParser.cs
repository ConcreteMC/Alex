using System.Text;
using Alex.Networking.Java.Models.Commands.Nodes;

namespace Alex.Networking.Java.Models.Commands.Properties
{
	public class ArgumentParser
	{
		public CommandNode Parent { get; internal set; }
		public string Name { get; set; }

		public ArgumentParser(string name)
		{
			Name = name;
		}
		
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

			return base.ToString();
		}
	}

	public class StringArgumentParser : ArgumentParser
	{
		public StringMode Mode { get; }
		/// <inheritdoc />
		public StringArgumentParser(string name, StringMode mode) : base(name)
		{
			Mode = mode;
		}

		public enum StringMode
		{
			SingleWord,
			QuotablePhrase,
			GreedyPhrase
		}
	}

	public class MessageArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public MessageArgumentParser(string name) : base(name)
		{
			
		}
	}
	
	public class ObjectiveArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public ObjectiveArgumentParser(string name) : base(name)
		{
			
		}
	}
}