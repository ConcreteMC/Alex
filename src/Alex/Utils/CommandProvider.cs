using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Data;
using NLog;
using Org.BouncyCastle.Ocsp;

namespace Alex.Utils
{
	public delegate void OnCommandMatch(int start, int length, TabCompleteMatch[] matches);
	public abstract class CommandProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(CommandProvider));
		private List<Command> _commands = new List<Command>();
		public CommandProvider()
		{
			
		}

		public int Count => _commands.Count;
		
		public void Register(Command command)
		{
			if (!_commands.Contains(command))
			{
				_commands.Add(command);
			}
		}

		public void Match(string input, OnCommandMatch onMatch)
		{
			var split = input.Split(' ');

			if (split.Length == 0)
				return;

			var last = split.Last();
			var index = input.IndexOf(last, StringComparison.InvariantCultureIgnoreCase);

			List<TabCompleteMatch> matches = new List<TabCompleteMatch>();
			foreach (var command in _commands.Where(x => x.IsMatch(last)))
			{
				matches.AddRange(command.GetMatches(last));
			}

			if (matches.Count > 0)
			{
				onMatch?.Invoke(index, last.Length, matches.ToArray());
			}
			else
			{
				DoMatch(input, onMatch);
			}
		}
		
		public abstract void DoMatch(string input, OnCommandMatch callback);
	}

	public class Command
	{
		public string[] Aliases { get; }
		public List<CommandProperty> Properties { get; }

		public Command(params string[] aliases)
		{
			Aliases = aliases;
			Properties = new List<CommandProperty>();
		}

		public void AddProperty(CommandProperty property)
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
						Match = x
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
	}

	public class CommandProperty
	{
		public string Name { get; }
		public bool Required { get; }
		public CommandProperty(string name, bool required = true)
		{
			Name = name;
			Required = required;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Required ? $"<{Name}>" : $"[{Name}]";
		}
	}
}