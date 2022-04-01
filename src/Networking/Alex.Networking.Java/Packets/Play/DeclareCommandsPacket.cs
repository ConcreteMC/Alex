using System;
using System.Collections.Generic;
using Alex.Networking.Java.Commands;
using Alex.Networking.Java.Commands.Nodes;
using Alex.Networking.Java.Commands.Parsers;
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
				var flags = (byte)stream.ReadByte();
				var nodeType = (CommandNodeType)(flags & 0x03);

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

				CommandNode node;

				switch (nodeType)
				{
					case CommandNodeType.Root:
						node = new CommandNode(CommandNodeType.Root);

						break;

					case CommandNodeType.Literal:
						node = new LiteralCommandNode(name) { };

						break;

					case CommandNodeType.Argument:
						var acn = new ArgumentCommandNode(name) { Parser = GetParser(stream) };

						acn.Parser.Parent = acn;

						if ((flags & 0x10) != 0)
						{
							acn.SuggestionType = stream.ReadString();
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

				Log.Debug(
					$"Type={nodeType.ToString()} Index={i} Name={name ?? "null"}, isExecuteable={node.IsExecutable} hasRedirect={node.HasRedirect} redirectIndex={node.RedirectIndex} children={children.Length}");

				nodes.Add(node);
			}

			RootIndex = stream.ReadVarInt();
			Nodes.AddRange(nodes);
		}

		private ArgumentParser GetParser(MinecraftStream stream)
		{
			string parser = stream.ReadString();

			switch (parser)
			{
				case "minecraft:resource_location":
				{
					return new ResourceLocationArgumentParser(parser);
				}

				case "minecraft:block_pos":
				{
					return new BlockPositionArgumentParser(parser);
				}

				case "minecraft:vec2":
				{
					return new Vector2ArgumentParser(parser);
				}

				case "minecraft:vec3":
				{
					return new Vector3ArgumentParser(parser);
				}

				case "minecraft:column_pos":
				{
					return new ColumnPositionArgumentParser(parser);
				}

				case "brigadier:bool":
				{
					return new BoolArgumentParser(parser);
				}

				case "brigadier:double":
				{
					DoubleArgumentParser cp = new DoubleArgumentParser(parser);
					var dFlags = (byte)stream.ReadByte();
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

					return cp;
				}

				case "brigadier:float":
				{
					FloatArgumentParser cp = new FloatArgumentParser(parser);
					var dFlags = (byte)stream.ReadByte();
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

					return cp;
				}

				case "brigadier:integer":
				{
					IntegerArgumentParser cp = new IntegerArgumentParser(parser);
					var dFlags = (byte)stream.ReadByte();
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

					return cp;
				}

				case "brigadier:string":
				{
					var a = stream.ReadVarInt();

					return (new StringArgumentParser(parser, (StringArgumentParser.StringMode)a));
				}

				case "minecraft:entity":
				{
					var entityFlags = (byte)stream.ReadByte();

					return (new EntityArgumentParser(parser, entityFlags));
				}

				//	case "minecraft:game_profile":
				//		break;

				//	case "minecraft:block_pos":
				//		break;

				case "minecraft:score_holder":
				{
					var scoreHolderFlags = (byte)stream.ReadByte();

					return (new ScoreHolderArgumentParser(parser, scoreHolderFlags));
				}

				case "minecraft:range":
				{
					bool allowDecimals = stream.ReadBool();

					if (allowDecimals)
					{
						return (new DoubleArgumentParser(parser) { });
					}
					
					return (new IntegerArgumentParser(parser) { });
				}

				case "minecraft:message":
				{
					return (new MessageArgumentParser(parser));
				}

				case "minecraft:objective":
				{
					return (new ObjectiveArgumentParser(parser));
				}

				default:
					Log.Warn($"Unknown parser: {parser}");
					return (new StringArgumentParser(parser, StringArgumentParser.StringMode.SingleWord));
			}
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}