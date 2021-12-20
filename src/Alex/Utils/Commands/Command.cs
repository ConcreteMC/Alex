using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.Common.Commands.Parsers;
using Alex.Common.Data;

namespace Alex.Utils.Commands
{
	public class Command
	{
		public string[] Aliases { get; }
		public string Description { get; set; } = null;
		public List<IArgumentParser> Properties { get; }

		public Command(params string[] aliases)
		{
			Aliases = aliases;
			Properties = new List<IArgumentParser>();
		}

		public void AddProperty(IArgumentParser property)
		{
			Properties.Add(property);
		}

		public bool IsMatch(string input)
		{
			return Aliases.Any(x => x.StartsWith(input, StringComparison.InvariantCultureIgnoreCase));// Alias.StartsWith(input, StringComparison.InvariantCultureIgnoreCase);
		}

		public TabCompleteMatch[] GetMatches(string input)
		{
			return Aliases.Where(x => x.StartsWith(input, StringComparison.InvariantCultureIgnoreCase)).Select(
				x =>
				{
					return new TabCompleteMatch()
					{
						Match = x,
						Description = Description
					};
				}).ToArray();
		}

		public IEnumerable<string> Describe()
		{
			StringBuilder sb = new StringBuilder();

			foreach (var alias in Aliases)
			{
				sb.Clear();
				
				sb.Append('/');
				sb.Append(alias);

				foreach (var property in Properties)
				{
					sb.Append(' ');
					sb.Append(property.ToString());
				}

				yield return sb.ToString();
			}

			//return sb.ToString();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Join(' ', Describe());
		}
	}
}