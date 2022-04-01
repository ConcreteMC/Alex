using Alex.Interfaces;
using Alex.Networking.Java.Commands.Parsers;

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

		public virtual bool TryParse(ISeekableTextReader reader)
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