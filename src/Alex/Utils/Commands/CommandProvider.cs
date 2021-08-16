using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alex.Common.Commands;
using Alex.Common.Commands.Nodes;
using Alex.Common.Data;
using Alex.Worlds;
using NLog;

namespace Alex.Utils.Commands
{
	public delegate void OnCommandMatch(int start, int length, TabCompleteMatch[] matches);
	public abstract class CommandProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(CommandProvider));
		//private List<Command> _commands = new List<Command>();

		private World _world;

		public int RootIndex { get; set; } = 0;
		public CommandNode[] Nodes { get; set; }
		protected CommandProvider(World world)
		{
			_world = world;
		}

		public int Count => Nodes.Count(x => x.IsExecutable);
		public bool Enabled { get; set; } = true;

		public void Reset()
		{
			//_commands.Clear();
		}
		
		//public void Register(Command command)
		//{
		//	if (!_commands.Contains(command))
		//	{
		//		_commands.Add(command);
		//		Log.Debug($"Registered command: {command.ToString()}");
		//	}
		//}

		private CommandNode Match(CommandNode node, SeekableTextReader input)
		{
			CommandNode bestMatch = null;
			double bestSimilarity = 0d;
			int bestPosition = input.Position;
			int startPosition = input.Position;
			string stringMatch = null;
			
			foreach (var childIndex in node.Children)
			{
				var position = input.Position;

				try
				{
					var childNode = Nodes[childIndex];

					switch (childNode.NodeType)
					{
						case CommandNodeType.Argument:
							if (childNode is ArgumentCommandNode acn)
							{
								//var inputPosition = input.Position;
								if (acn.Parser.TryParse(input, out string match))
								{
									var similarity = match.CalculateSimilarity(acn.Name);

									if (similarity > bestSimilarity)
									{
										bestMatch = acn;
										bestSimilarity = similarity;
										bestPosition = input.Position - 1;
										stringMatch = match;
									}
								}
							}

							break;

						case CommandNodeType.Literal:
							if (childNode is LiteralCommandNode lcn)
							{
								var length = input.ReadSingleWord(out var textInput);
								if (length > 0 && lcn.Name.StartsWith(textInput, StringComparison.InvariantCultureIgnoreCase))
								{
									var similarity = textInput.CalculateSimilarity(lcn.Name);

									if (similarity > bestSimilarity)
									{
										bestMatch = lcn;
										bestSimilarity = similarity;
										bestPosition = input.Position - 1;
										stringMatch = lcn.Name;
									}
								}
							}

							break;

						case CommandNodeType.Root:
							break;
					}
				}
				finally
				{
					input.Position = position;
				}
			}

			input.Position = bestPosition;

			return bestMatch;
		}
		
		public void Match(string input, OnCommandMatch onMatch)
		{
		//	var split = input.Split(' ');

		//	if (split.Length == 0)
			//	return;

			if (string.IsNullOrWhiteSpace(input))
			{
				Log.Warn($"Invalid input: {input}");
				return;
			}

			var root = Nodes[RootIndex];

			int startOfMatch = 0;
			int length = 0;
			CommandNode executeable = null;
			CommandNode cn = root;

			if (cn == null)
			{
				Log.Warn($"Root node was null for input: {input}");
				return;
			}
			
			List<TabCompleteMatch> matches = new List<TabCompleteMatch>();
			using (SeekableTextReader sr = new SeekableTextReader(input))
			{
				while (cn != null && sr.Position < sr.Length)
				{
					var startPosition = sr.Position;
					try
					{
						if (cn.HasRedirect)
							cn = Nodes[cn.RedirectIndex];
						
						cn = Match(cn, sr);
						var endPosition = sr.Position;

						if (cn != null && endPosition > startPosition)
						{
							//if (cn.IsExecutable)
							//	executeable = cn;

							//length = endPosition - startPosition;

							if (cn.IsExecutable && cn is NamedCommandNode ncn)
							{
								startOfMatch = startPosition;
								length = endPosition - (startPosition - 1);
								executeable = ncn;

								if (executeable != null)
								{
									matches.Add(new TabCompleteMatch() { Match = ncn.Name, Description = "Unknown" });
								}
							}
						}

						//cn = match;;
					}
					finally
					{
						//sr.Position = startPosition;
					}
				}
				
				if (matches.Count > 0)
				{
					onMatch?.Invoke(
						startOfMatch, length,
						matches.ToArray());

					return;
				}
			}
		}
		
		public abstract void DoMatch(string input, OnCommandMatch callback);
	}
}