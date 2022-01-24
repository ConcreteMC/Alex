using System.IO;
using Alex.Common.Commands.Parsers;
using Alex.Common.Data;
using NLog.Fluent;

namespace Alex.Utils.Commands
{
	public class CommandProperty : IArgumentParser
	{
		public string Name { get; }
		public bool Required { get; }

		public string TypeIdentifier { get; set; }

		public CommandProperty(string name, bool required = true, string typeIdentifier = "Unknown")
		{
			Name = name;
			Required = required;
			TypeIdentifier = typeIdentifier;
		}

		public virtual bool TryParse(SeekableTextReader reader)
		{
			if (reader.ReadSingleWord(out _) > 0)
			{
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Required ? $"<{Name}: {TypeIdentifier}>" : $"[{Name}: {TypeIdentifier}]";
		}
	}
}