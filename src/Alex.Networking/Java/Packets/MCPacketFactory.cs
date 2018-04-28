using System;
using Alex.Networking.Java.Packets.Handshake;
using Alex.Networking.Java.Packets.Login;
using Alex.Networking.Java.Packets.Play;
using Alex.Networking.Java.Packets.Status;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets
{
	public static class MCPacketFactory
	{
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
			Register(Direction.ServerBound, ConnectionState.Login, 0x00, () => new LoginStartPacket());
			Register(Direction.ServerBound, ConnectionState.Login, 0x01, () => new EncryptionResponsePacket());

			Register(Direction.ClientBound, ConnectionState.Login, 0x00, () => new DisconnectPacket());
			Register(Direction.ClientBound, ConnectionState.Login, 0x01, () => new EncryptionRequestPacket());
			Register(Direction.ClientBound, ConnectionState.Login, 0x02, () => new LoginSuccessPacket());
			Register(Direction.ClientBound, ConnectionState.Login, 0x03, () => new SetCompressionPacket());
		}

		private static void RegisterPlay()
		{
			Register(Direction.ServerBound, ConnectionState.Play, 0x0B, () => new KeepAlivePacket()
			{
				PacketId = 0x0B
			});
			Register<ClientSettingsPacket>(Direction.ServerBound, ConnectionState.Play, 0x03);
			Register<ClientStatusPacket>(Direction.ServerBound, ConnectionState.Play, 0x02);
			Register<ChatMessagePacket>(Direction.ServerBound, ConnectionState.Play, 0x01);

			Register(Direction.ClientBound, ConnectionState.Play, 0x03, () => new SpawnMob());
			Register(Direction.ClientBound, ConnectionState.Play, 0x05, () => new SpawnPlayerPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x0d, () => new ServerDifficultyPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x0E, () => new ChatMessagePacket()
			{
				ServerBound = false
			});
			Register(Direction.ClientBound, ConnectionState.Play, 0x19, () => new PluginMessagePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x1E, () => new UnloadChunk());
			Register(Direction.ClientBound, ConnectionState.Play, 0x20, () => new KeepAlivePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x21, () => new ChunkDataPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x24, () => new JoinGamePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x3C, () => new HeldItemChangePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x2D, () => new PlayerAbilitiesPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x2F, () => new PlayerListItemPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x31, () => new PlayerPositionAndLookPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x49, () => new TimeUpdatePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x27, () => new EntityRelativeMove());
			Register(Direction.ClientBound, ConnectionState.Play, 0x28, () => new EntityLookAndRelativeMove());
			Register(Direction.ClientBound, ConnectionState.Play, 0x29, () => new EntityLook());
			Register(Direction.ClientBound, ConnectionState.Play, 0x38, () => new EntityHeadLook());
			Register(Direction.ClientBound, ConnectionState.Play, 0x40, () => new EntityVelocity());
			Register(Direction.ClientBound, ConnectionState.Play, 0x51, () => new EntityPropertiesPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x4F, () => new EntityTeleport());
			Register(Direction.ClientBound, ConnectionState.Play, 0x34, () => new DestroyEntitiesPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x15, () => new WindowItems());
			Register(Direction.ClientBound, ConnectionState.Play, 0x17, () => new SetSlot());
			Register(Direction.ClientBound, ConnectionState.Play, 0x1C, () => new EntityStatusPacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x0B, () => new BlockChangePacket());
			Register(Direction.ClientBound, ConnectionState.Play, 0x0F, () => new MultiBlockChange());

			Register(Direction.ClientBound, ConnectionState.Play, 0x10, () => new TabCompleteClientBound());
		}
	}
}
