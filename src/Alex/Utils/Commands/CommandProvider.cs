using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alex.API.Data;
using Alex.Worlds;
using NLog;

namespace Alex.Utils.Commands
{
	public delegate void OnCommandMatch(int start, int length, TabCompleteMatch[] matches);
	public abstract class CommandProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(CommandProvider));
		private List<Command> _commands = new List<Command>();

		private World _world;
		public CommandProvider(World world)
		{
			_world = world;
		}

		public int Count => _commands.Count;

		public void Reset()
		{
			_commands.Clear();
		}
		
		public void Register(Command command)
		{
			if (!_commands.Contains(command))
			{
				_commands.Add(command);
				Log.Debug($"Registered command: {command.ToString()}");
			}
		}

		public void Match(string input, OnCommandMatch onMatch)
		{
			var split = input.Split(' ');

			if (split.Length == 0)
				return;

			var first = split.First();
			var matchingCommands = _commands.Where(x => x.IsMatch(first)).ToArray();
			var remainder = string.Join(' ', split.Skip(1));

			List<TabCompleteMatch> matches = new List<TabCompleteMatch>();
			if (split.Length == 1)
			{
				foreach (var command in matchingCommands)
				{
					var firstMatchingAlias = command.GetMatches(first);
					matches.AddRange(firstMatchingAlias);
				}

				if (matches.Count > 0)
				{
					onMatch?.Invoke(0, first.Length, matches.ToArray());
				}
				else
				{
					DoMatch(input, onMatch);
				}

				return;
			}
			
			Command bestMatch = null;
			int maxMatchCount = -1;
			int startIndex = 0;
			int length = 0;

			if (remainder.Length > 0)
			{
				using (SeekableTextReader sr = new SeekableTextReader(input))
				{
					sr.Position += first.Length + 1;
					foreach (var command in matchingCommands)
					{
						bool isValid = true;
						int matchCount = 0;
						sr.Position = 0;

						int startPos = 0;
						int cmdLength = 0;

						//
						for (int i = 0; i < command.Properties.Count; i++)
						{
							if (sr.Position == sr.Length - 1) //Reached end of text.
							{
								break;
							}

							while (char.IsWhiteSpace((char) sr.Peek()))
							{
								sr.Read();
							}

							startPos = sr.Position;
							var property = command.Properties[i];

							if (!property.TryParse(sr))
							{
								isValid = false;

								break;
							}

							matchCount++;

							var endPosition = sr.Position;

							cmdLength = endPosition - startPos;
						}

						if (isValid)
						{
							if (matchCount > maxMatchCount)
							{
								bestMatch = command;
								maxMatchCount = matchCount;
								startIndex = startPos;
								length = cmdLength;
							}
							//break;
						}
					}
				}
			}
			else
			{
				foreach (var command in matchingCommands)
				{
					var firstMatchingAlias = command.GetMatches(first);
					matches.AddRange(firstMatchingAlias);
				}
			}

			if (bestMatch != null)
			{
				Log.Info($"Best match: {bestMatch.ToString()}");
				//if (startIndex < )
			}
			else
			{
				length = first.Length;
			}
			
			if (matches.Count > 0)
			{
				Log.Info($"Found {matches.Count} matches for \"{input}\"!");
				onMatch?.Invoke(startIndex, length, matches.ToArray());
			}
			else
			{
				DoMatch(input, onMatch);
			}
		}
		
		public abstract void DoMatch(string input, OnCommandMatch callback);
	}
}