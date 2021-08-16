using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alex.Common.Data;
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
		protected CommandProvider(World world)
		{
			_world = world;
		}

		public int Count => _commands.Count;
		public bool Enabled { get; set; } = true;

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

			Log.Info(
				$"Matching... (Command={first}) (Remainder={remainder}) MatchingCommands={matchingCommands.Length}");
			List<TabCompleteMatch> matches = new List<TabCompleteMatch>();
			
			//Resolved command alias, return all matches
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
				using (SeekableTextReader sr = new SeekableTextReader(remainder))
				{
				//	sr.Position += first.Length + 1;
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
							if (sr.Position == sr.Length) //Reached end of text.
							{
								break;
							}

							while (char.IsWhiteSpace((char) sr.Peek()))
							{
								sr.Read();
							}

							startPos = sr.Position;
							var property = command.Properties[i];
							property.Matches = new string[0];
							
							if (!property.TryParse(sr))
							{
								isValid = false;
Log.Debug($"Property \"{property.Name}\" does not match \"{sr}\"");
								break;
							}

							if (property.Matches.Length > 0)
							{
								matches.AddRange(property.Matches.Select(x => new TabCompleteMatch()
								{
									Description = property.Name,
									Match = x,
								}));
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
				var last = split.Last();
				startIndex = input.Length - last.Length;// first.Length + 1;
				length = last.Length;
				
				foreach (var command in matchingCommands)
				{
					if (command.Properties.Count < split.Length)
					{
						Log.Warn($"Command properties: {command.Properties.Count}, split: {split.Length}");
						continue;
					}

					//for (int i = split.Length - 1; i < command.Properties.Count; i++)
					{
						var property = command.Properties[split.Length - 1];
						Log.Info($"Property: {property.Name}");
						if (property is EnumCommandProperty enumProp)
						{
							matches.AddRange(enumProp.Options.Select(x => new TabCompleteMatch()
							{
								Match = x,
							}));
						}
					}
					//var firstMatchingAlias = command.GetMatches(first);
					//matches.AddRange(firstMatchingAlias);
				}
			}

			if (bestMatch != null)
			{
				Log.Info($"Best match: {bestMatch.ToString()} (remainder: {remainder})");
				//matches.AddRange(bestMatch.Properties..GetMatches(remainder));
				//if (startIndex < )
			}
			else
			{
				//length = first.Length;
			}
			
			if (matches.Count > 0)
			{
				Log.Info($"Found {matches.Count} matches for \"{input}\"! (StartIndex={startIndex} Length={length})");
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