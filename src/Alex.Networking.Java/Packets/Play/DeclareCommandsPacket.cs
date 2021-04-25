using System;
using System.Collections.Generic;
using System.Text;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class CommandNode
	{
		public CommandNodeType NodeType { get; }
		public bool IsExecutable { get; set; } = false;
		public CommandNode(CommandNodeType type)
		{
			NodeType = type;
		}
		
		public int[] Children { get; set; }
	}
	
	public class NamedCommandNode : CommandNode
	{
		public string Name { get; }
		/// <inheritdoc />
		public NamedCommandNode(CommandNodeType type, string name) : base(type)
		{
			Name = name;
		}
	}

	public class ArgumentCommandNode : NamedCommandNode
	{
		public string Parser { get; set; }
		public List<CommandProperty> Properties { get; set; }
		/// <inheritdoc />
		public ArgumentCommandNode(string name) : base(CommandNodeType.Argument, name)
		{
			
		}
	}
	
	public class LiteralCommandNode : NamedCommandNode
	{
		/// <inheritdoc />
		public LiteralCommandNode(string name) : base(CommandNodeType.Literal, name)
		{
			
		}
	}

	public class CommandProperty
	{
		public CommandNode Parent { get; internal set; }
		public string Name { get; set; }

		public CommandProperty(string name)
		{
			Name = name;
		}
		
		/// <inheritdoc />
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (Parent.IsExecutable)
			{
				sb.Append('[');
			}
			else
			{
				sb.Append('<');
			}

			sb.AppendFormat("{0}", Name);

			if (Parent.IsExecutable)
			{
				sb.Append(']');
			}
			else
			{
				sb.Append('>');
			}

			return base.ToString();
		}
	}

	public class RangeCommandProperty<T> : CommandProperty
	{
		public byte Flags { get; set; }
		public T? Min { get; set; }
		public T? Max { get; set; }

		/// <inheritdoc />
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (Parent.IsExecutable)
			{
				sb.Append('[');
			}
			else
			{
				sb.Append('<');
			}

			sb.AppendFormat("{0}:{1}", Name, typeof(T).Name);

			if (Parent.IsExecutable)
			{
				sb.Append(']');
			}
			else
			{
				sb.Append('>');
			}

			return base.ToString();
		}

		/// <inheritdoc />
		public RangeCommandProperty(string name) : base(name) { }
	}

	public class DoubleCommandProperty : RangeCommandProperty<double>
	{
		/// <inheritdoc />
		public DoubleCommandProperty(string name) : base(name) { }
	}
	
	public class FloatCommandProperty : RangeCommandProperty<float>
	{
		/// <inheritdoc />
		public FloatCommandProperty(string name) : base(name) { }
	}
	
	public class IntegerCommandProperty : RangeCommandProperty<int>
	{
		/// <inheritdoc />
		public IntegerCommandProperty(string name) : base(name) { }
	}
	
	public class DeclareCommandsPacket : Packet<DeclareCommandsPacket>
	{
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

				List<CommandProperty> properties = new List<CommandProperty>();
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
							DoubleCommandProperty cp = new DoubleCommandProperty(parser);
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
						} break;

						case "brigadier:float":
						{
							FloatCommandProperty cp = new FloatCommandProperty(parser);
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
						} break;

						case "brigadier:integer":
						{
							IntegerCommandProperty cp = new IntegerCommandProperty(parser);
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
						} break;

						case "brigadier:string":
						{
							var a = stream.ReadVarInt();
						} break;

						case "minecraft:entity":
						{
							stream.ReadByte();
						} break;
						
					//	case "minecraft:game_profile":
					//		break;
						
					//	case "minecraft:block_pos":
					//		break;

						case "minecraft:score_holder":
						{
							stream.ReadByte();
						} break;

						case "minecraft:range":
						{
							stream.ReadBool();
						} break;
						default:
							properties.Add(new CommandProperty(parser));
							break;
					}
				}

				if ((flags & 0x10) != 0)
				{
					stream.ReadString();
				}

				CommandNode node;
				switch (nodeType)
				{
					case CommandNodeType.Root:
						node = new CommandNode(CommandNodeType.Root);
						break;

					case CommandNodeType.Literal:
						node = new LiteralCommandNode(name)
						{
							
						};
						break;

					case CommandNodeType.Argument:
						node = new ArgumentCommandNode(name)
						{
							Parser = parser,
							Properties = properties
						};

						foreach (var property in properties)
						{
							property.Parent = node;
						}
						break;
					default:
						throw new NotSupportedException($"Nodetype not supported");
				}

				node.IsExecutable = (flags & 0x04) != 0;
				node.Children = children;
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

	public enum CommandNodeType
	{
		Root = 0,
		Literal = 1,
		Argument = 2
	}
}