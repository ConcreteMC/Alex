using System;
using System.Collections.Generic;
using Alex.Common.Commands;
using Alex.Common.Commands.Nodes;
using Alex.Common.Commands.Properties;
using Alex.Networking.Java.Util;
using NLog;
using NLog.Fluent;

namespace Alex.Networking.Java.Packets.Play
{
	public class DeclareCommandsPacket : Packet<DeclareCommandsPacket>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(DeclareCommandsPacket));
		public List<CommandNode> Nodes { get; set; } = new List<CommandNode>();
		public int RootIndex { get; set; } = 0;

		/// <inheritdoc />
		protected override void ResetPacket()
		{
			base.ResetPacket();
			Nodes.Clear();
			RootIndex = 0;
		}

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			var count = stream.ReadVarInt();

			List<CommandNode> nodes = new List<CommandNode>();

			for (int i = 0; i < count; i++)
			{
				var flags = (byte) stream.ReadByte();
				var nodeType = (CommandNodeType) (flags & 0x03);

				var childCount = stream.ReadVarInt();
				int[] children = new int[childCount];

				for (int c = 0; c < children.Length; c++)
				{
					children[c] = stream.ReadVarInt();
				}

				int? redirectNode = null;

				if ((flags & 0x08) != 0)
				{
					redirectNode = stream.ReadVarInt();
				}

				string name = null;

				if (nodeType == CommandNodeType.Argument || nodeType == CommandNodeType.Literal)
				{
					name = stream.ReadString();
				}

				List<ArgumentParser> properties = new List<ArgumentParser>();
				string parser = null;

				if (nodeType == CommandNodeType.Argument)
				{
					parser = stream.ReadString();

					switch (parser)
					{
						//	case "brigadier:bool":
						//	stream.ReadBool();
						//		break;

						case "brigadier:double":
						{
							DoubleArgumentParser cp = new DoubleArgumentParser(parser);
							var dFlags = (byte) stream.ReadByte();
							cp.Flags = dFlags;

							if ((dFlags & 0x01) != 0)
							{
								var dMin = stream.ReadDouble();
								cp.Min = dMin;
							}

							if ((dFlags & 0x02) != 0)
							{
								var dMax = stream.ReadDouble();
								cp.Max = dMax;
							}

							properties.Add(cp);
						}

							break;

						case "brigadier:float":
						{
							FloatArgumentParser cp = new FloatArgumentParser(parser);
							var dFlags = (byte) stream.ReadByte();
							cp.Flags = dFlags;

							if ((dFlags & 0x01) != 0)
							{
								var dMin = stream.ReadFloat();
								cp.Min = dMin;
							}

							if ((dFlags & 0x02) != 0)
							{
								var dMax = stream.ReadFloat();
								cp.Max = dMax;
							}

							properties.Add(cp);
						}

							break;

						case "brigadier:integer":
						{
							IntegerArgumentParser cp = new IntegerArgumentParser(parser);
							var dFlags = (byte) stream.ReadByte();
							cp.Flags = dFlags;

							if ((dFlags & 0x01) != 0)
							{
								var dMin = stream.ReadInt();
								cp.Min = dMin;
							}

							if ((dFlags & 0x02) != 0)
							{
								var dMax = stream.ReadInt();
								cp.Max = dMax;
							}

							properties.Add(cp);
						}

							break;

						case "brigadier:string":
						{
							var a = stream.ReadVarInt();
							properties.Add(new StringArgumentParser(name, (StringArgumentParser.StringMode) a));
						}

							break;

						case "minecraft:entity":
						{
							var entityFlags = (byte) stream.ReadByte();
							properties.Add(new EntityArgumentParser(name, entityFlags));
						}

							break;

						//	case "minecraft:game_profile":
						//		break;

						//	case "minecraft:block_pos":
						//		break;

						case "minecraft:score_holder":
						{
							var scoreHolderFlags = (byte) stream.ReadByte();
							properties.Add(new ScoreHolderArgumentParser(name, scoreHolderFlags));
						}

							break;

						case "minecraft:range":
						{
							bool allowDecimals = stream.ReadBool();

							if (allowDecimals)
							{
								properties.Add(new DoubleArgumentParser(parser) { });
							}
							else
							{
								properties.Add(new IntegerArgumentParser(parser) { });
							}
						}

							break;

						case "minecraft:message":
						{
							properties.Add(new MessageArgumentParser(name));
						}

							break;

						case "minecraft:objective":
						{
							properties.Add(new ObjectiveArgumentParser(name));
						}

							break;

						default:
							Log.Warn($"Unknown parser: {parser}");
							properties.Add(new ArgumentParser(parser));

							break;
					}
				}

				CommandNode node;

				string suggestionType = "n/a";

				switch (nodeType)
				{
					case CommandNodeType.Root:
						node = new CommandNode(CommandNodeType.Root);

						break;

					case CommandNodeType.Literal:
						node = new LiteralCommandNode(name) { };

						break;

					case CommandNodeType.Argument:
						var acn = new ArgumentCommandNode(name) {Parser = parser, Parsers = properties};

						if ((flags & 0x10) != 0)
						{
							suggestionType = acn.SuggestionType = stream.ReadString();
						}

						foreach (var property in properties)
						{
							property.Parent = acn;
						}

						node = acn;

						break;

					default:
						throw new NotSupportedException($"Nodetype not supported");
				}

				node.RedirectIndex = redirectNode.GetValueOrDefault(-1);
				node.HasRedirect = (flags & 0x08) != 0;
				node.IsExecutable = (flags & 0x04) != 0;
				node.Children = children;

				Log.Info(
					$"Type={nodeType.ToString()} Index={i} Name={name ?? "null"}, properties={properties.Count} isExecuteable={node.IsExecutable} hasRedirect={node.HasRedirect} redirectIndex={node.RedirectIndex} children={children.Length} suggestionType={suggestionType}");

				nodes.Add(node);
			}

			RootIndex = stream.ReadVarInt();
			Nodes.AddRange(nodes);
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}