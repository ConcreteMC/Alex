using System;
using System.Collections.Generic;
using Alex.Networking.Java.Packets.Handshake;
using Alex.Networking.Java.Packets.Login;
using Alex.Networking.Java.Packets.Play;
using Alex.Networking.Java.Packets.Status;
using Alex.Networking.Java.Util;
using NLog;

namespace Alex.Networking.Java.Packets
{
	public static class MCPacketFactory
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MCPacketFactory));
		private static PacketFactory<int, MinecraftStream, Packet> HandshakeFactory { get; }
		private static PacketFactory<int, MinecraftStream, Packet> StatusFactory { get; }
		private static PacketFactory<int, MinecraftStream, Packet> LoginFactory { get; }
		private static PacketFactory<int, MinecraftStream, Packet> PlayFactory { get; }

		private static PacketFactory<int, MinecraftStream, Packet> ServerHandshakeFactory { get; }
		private static PacketFactory<int, MinecraftStream, Packet> ServerStatusFactory { get; }
		private static PacketFactory<int, MinecraftStream, Packet> ServerLoginFactory { get; }
		private static PacketFactory<int, MinecraftStream, Packet> ServerPlayFactory { get; }

		static MCPacketFactory()
		{
			HandshakeFactory = new PacketFactory<int, MinecraftStream, Packet>();
			StatusFactory = new PacketFactory<int, MinecraftStream, Packet>();
			LoginFactory = new PacketFactory<int, MinecraftStream, Packet>();
			PlayFactory = new PacketFactory<int, MinecraftStream, Packet>();

			ServerHandshakeFactory = new PacketFactory<int, MinecraftStream, Packet>();
			ServerStatusFactory = new PacketFactory<int, MinecraftStream, Packet>();
			ServerLoginFactory = new PacketFactory<int, MinecraftStream, Packet>();
			ServerPlayFactory = new PacketFactory<int, MinecraftStream, Packet>();
		}

		internal static void Register<TPacket>(Direction direction, ConnectionState state, int packetId) where TPacket : Packet, new()
		{
			Register(direction, state, packetId, () => new TPacket());
		}

		internal static void Register<TPacket>(Direction direction, ConnectionState state, int packetId, Func<TPacket> createFunc) where TPacket : Packet
		{
			if (direction == Direction.ServerBound)
			{
				switch (state)
				{
					case ConnectionState.Handshake:
						HandshakeFactory.Register(packetId, createFunc);
						break;
					case ConnectionState.Status:
						StatusFactory.Register(packetId, createFunc);
						break;
					case ConnectionState.Login:
						LoginFactory.Register(packetId, createFunc);
						break;
					case ConnectionState.Play:
						PlayFactory.Register(packetId, createFunc);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(state), state, null);
				}
			}
			else
			{
				switch (state)
				{
					case ConnectionState.Handshake:
						ServerHandshakeFactory.Register(packetId, createFunc);
						break;
					case ConnectionState.Status:
						ServerStatusFactory.Register(packetId, createFunc);
						break;
					case ConnectionState.Login:
						ServerLoginFactory.Register(packetId, createFunc);
						break;
					case ConnectionState.Play:
						ServerPlayFactory.Register(packetId, createFunc);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(state), state, null);
				}
			}
		}

		private static TPacket CreatePacket<TPacket>(Type type, PacketFactory<int, MinecraftStream, Packet> factory)
			where TPacket : Packet
		{
			int packetId;
			if (factory.TryGetPacketId(type, out packetId))
			{
				Packet packet;
				if (factory.TryGetPacket(packetId, out packet))
				{
					packet.PacketId = packetId;
					return (TPacket)packet;
				}
			}

			return null;
		}

		public static TPacket CreatePacket<TPacket>(Direction direction, ConnectionState state) where TPacket : Packet
		{
			if (direction == Direction.ServerBound)
			{
				Type type = typeof(TPacket);
				switch (state)
				{
					case ConnectionState.Handshake:
						return CreatePacket<TPacket>(type, HandshakeFactory);
					case ConnectionState.Status:
						return CreatePacket<TPacket>(type, StatusFactory);
					case ConnectionState.Login:
						return CreatePacket<TPacket>(type, LoginFactory);
					case ConnectionState.Play:
						return CreatePacket<TPacket>(type, PlayFactory);
					default:
						throw new ArgumentOutOfRangeException(nameof(state), state, null);
				}
			}
			else
			{
				Type type = typeof(TPacket);
				switch (state)
				{
					case ConnectionState.Handshake:
						return CreatePacket<TPacket>(type, ServerHandshakeFactory);
					case ConnectionState.Status:
						return CreatePacket<TPacket>(type, ServerStatusFactory);
					case ConnectionState.Login:
						return CreatePacket<TPacket>(type, ServerLoginFactory);
					case ConnectionState.Play:
						return CreatePacket<TPacket>(type, ServerPlayFactory);
					default:
						throw new ArgumentOutOfRangeException(nameof(state), state, null);
				}
			}
		}

		public static Packet GetPacket(Direction direction, ConnectionState state, int packetId)
		{
			bool success;
			Packet outPacket;
			if (direction == Direction.ServerBound)
			{
				switch (state)
				{
					case ConnectionState.Handshake:
						success = HandshakeFactory.TryGetPacket(packetId, out outPacket);
						break;
					case ConnectionState.Status:
						success = StatusFactory.TryGetPacket(packetId, out outPacket);
						break;
					case ConnectionState.Login:
						success = LoginFactory.TryGetPacket(packetId, out outPacket);
						break;
					case ConnectionState.Play:
						success = PlayFactory.TryGetPacket(packetId, out outPacket);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(state), state, null);
				}
			}
			else
			{
				switch (state)
				{
					case ConnectionState.Handshake:
						success = ServerHandshakeFactory.TryGetPacket(packetId, out outPacket);
						break;
					case ConnectionState.Status:
						success = ServerStatusFactory.TryGetPacket(packetId, out outPacket);
						break;
					case ConnectionState.Login:
						success = ServerLoginFactory.TryGetPacket(packetId, out outPacket);
						break;
					case ConnectionState.Play:
						success = ServerPlayFactory.TryGetPacket(packetId, out outPacket);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(state), state, null);
				}
			}

			if (!success) return null;
			outPacket.PacketId = packetId;
			return outPacket;
		}

		private static bool _loaded = false;
		public static void Load()
		{
			if (_loaded) return;

			RegisterHandshake();
			RegisterStatus();
			RegisterLogin();
			RegisterPlay();

			_loaded = true;
		}

		private static void RegisterHandshake()
		{
			Register(Direction.ServerBound, ConnectionState.Handshake, 0x00, () => new HandshakePacket());
		}

		private static void RegisterStatus()
		{
			Register(Direction.ServerBound, ConnectionState.Status, 0x00, () => new RequestPacket());
			Register(Direction.ClientBound, ConnectionState.Status, 0x00, () => new ResponsePacket());

			Register(Direction.ServerBound, ConnectionState.Status, 0x01, () => new PingPacket());
			Register(Direction.ClientBound, ConnectionState.Status, 0x01, () => new PingPacket());
		}

		private static void RegisterLogin()
		{
			//Register(Direction.ServerBound, ConnectionState.Login, 0x00, () => new LoginPluginMessagePacket());
			Register(Direction.ServerBound, ConnectionState.Login, 0x00, () => new LoginStartPacket());
			Register(Direction.ServerBound, ConnectionState.Login, 0x01, () => new EncryptionResponsePacket());
			Register(Direction.ServerBound, ConnectionState.Login, 0x02, () => new LoginPluginResponsePacket());

			//Register(Direction.ClientBound, ConnectionState.Login, 0x00, () => new LoginPluginMessagePacket());
			Register(Direction.ClientBound, ConnectionState.Login, 0x00, () => new DisconnectPacket());
			Register(Direction.ClientBound, ConnectionState.Login, 0x01, () => new EncryptionRequestPacket());
			Register(Direction.ClientBound, ConnectionState.Login, 0x02, () => new LoginSuccessPacket());
			Register(Direction.ClientBound, ConnectionState.Login, 0x03, () => new SetCompressionPacket());
			Register(Direction.ClientBound, ConnectionState.Login, 0x04, () => new LoginPluginRequestPacket());
		}

		private static void RegisterPlay()
		{
			Register(Direction.ServerBound, ConnectionState.Play, 0x0C, () => new KeepAlivePacket()
			{
				PacketId = 0x0B
			});
			Register<ClientSettingsPacket>(Direction.ServerBound, ConnectionState.Play, 0x03);
			Register<ClientStatusPacket>(Direction.ServerBound, ConnectionState.Play, 0x02);
			Register<ChatMessagePacket>(Direction.ServerBound, ConnectionState.Play, 0x01);

		//	Register(Direction.ClientBound, ConnectionState.Play, 0x03, () => new SpawnMob());
			Register(Direction.ClientBound, ConnectionState.Play, 0x05, () => new SpawnPlayerPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x0E, () => new ServerDifficultyPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x0F, () => new ChatMessagePacket()
			{
				ServerBound = false
			});
			Register(Direction.ClientBound, ConnectionState.Play, 0x19, () => new PluginMessagePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x1E, () => new UnloadChunk());
			Register(Direction.ClientBound, ConnectionState.Play, 0x21, () => new KeepAlivePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x22, () => new ChunkDataPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x26, () => new JoinGamePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x40, () => new HeldItemChangePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x32, () => new PlayerAbilitiesPacket());
			
			Register(Direction.ClientBound, ConnectionState.Play, 0x34, () => new PlayerListItemPacket()); //< -----
			
			Register(Direction.ClientBound, ConnectionState.Play, 0x36, () => new PlayerPositionAndLookPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x4F, () => new TimeUpdatePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x29, () => new EntityRelativeMove());
			Register(Direction.ClientBound, ConnectionState.Play, 0x2A, () => new EntityLookAndRelativeMove());
			Register(Direction.ClientBound, ConnectionState.Play, 0x2B, () => new EntityLook());
			Register(Direction.ClientBound, ConnectionState.Play, 0x3C, () => new EntityHeadLook());
			Register(Direction.ClientBound, ConnectionState.Play, 0x41, () => new EntityMetadataPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x46, () => new EntityVelocity());
			Register(Direction.ClientBound, ConnectionState.Play, 0x59, () => new EntityPropertiesPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x57, () => new EntityTeleport());
			Register(Direction.ClientBound, ConnectionState.Play, 0x35, () => new DestroyEntitiesPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x16, () => new WindowItems());
			Register(Direction.ClientBound, ConnectionState.Play, 0x17, () => new SetSlot());
			Register(Direction.ClientBound, ConnectionState.Play, 0x1C, () => new EntityStatusPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x0C, () => new BlockChangePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x10, () => new MultiBlockChange());
			Register(Direction.ClientBound, ConnectionState.Play, 0x25, () => new UpdateLightPacket());

			Register(Direction.ClientBound, ConnectionState.Play, 0x11, () => new TabCompleteClientBound());
			Register(Direction.ClientBound, ConnectionState.Play, 0x1F, () => new ChangeGameStatePacket());

			Register(Direction.ClientBound, ConnectionState.Play, 0x33, () => new CombatEventPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x24, () => new ParticlePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x47, () => new EntityEquipmentPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x3B, () => new RespawnPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x50, () => new TitlePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x1B, () => new DisconnectPacket());
			
			Register(Direction.ClientBound, ConnectionState.Play, 0x49, () => new UpdateHealthPacket());
			/*var deserial = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.PlayPacketID);
			foreach (var d in deserial)
			{
				try
				{
					int result = int.Parse(d.Key.Substring(2), NumberStyles.HexNumber);
					{
						_playPacketNames.Add(result, d.Value);
						if (!ServerPlayFactory.TryGetPacket(result, out Packet p))
						{
							Log.Info($"Unimplemented clientbound packet: 0x{result:X2} : {d.Value}");
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warn($"Failed to parse hex number...");
				}
			}*/
		}

		private static Dictionary<int, string> _playPacketNames = new Dictionary<int, string>();

		public static string GetPlayPacketName(int id)
		{
			if (PlayFactory.TryGetPacket(id, out var p))
			{
				return p.GetType().Name.Replace("Packet", "");
			}

			if (_playPacketNames.TryGetValue(id, out string result))
			{
				return result;
			}

			return "Unknown";
		}
	}
}
