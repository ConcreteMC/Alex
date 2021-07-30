using System.IO;
using Alex.Common.Data;
using NLog.Fluent;

namespace Alex.Utils.Commands
{
	public class CommandProperty
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

		public string[] Matches { get; set; } = new string[0];
		public virtual bool TryParse(SeekableTextReader reader)
		{
			if (reader.ReadSingleWord(out string result) > 0)
			{
				Matches = new string[] {result};
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