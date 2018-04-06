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
			if (direction == Direction.Client)
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
			if (direction == Direction.Client)
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
			if (direction == Direction.Client)
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

		public static void Load()
		{
			RegisterHandshake();
			RegisterStatus();
			RegisterLogin();
			RegisterPlay();
		}

		private static void RegisterHandshake()
		{
			Register(Direction.Client, ConnectionState.Handshake, 0x00, () => new HandshakePacket());
		}

		private static void RegisterStatus()
		{
			Register(Direction.Client, ConnectionState.Status, 0x00, () => new RequestPacket());
			Register(Direction.Client, ConnectionState.Status, 0x01, () => new PingPacket());

		}

		private static void RegisterLogin()
		{
			Register(Direction.Client, ConnectionState.Login, 0x00, () => new LoginStartPacket());
			Register(Direction.Client, ConnectionState.Login, 0x01, () => new EncryptionResponsePacket());

			Register(Direction.Server, ConnectionState.Login, 0x00, () => new DisconnectPacket());
			Register(Direction.Server, ConnectionState.Login, 0x01, () => new EncryptionRequestPacket());
			Register(Direction.Server, ConnectionState.Login, 0x02, () => new LoginSuccessPacket());
			Register(Direction.Server, ConnectionState.Login, 0x03, () => new SetCompressionPacket());
		}

		private static void RegisterPlay()
		{
			Register(Direction.Client, ConnectionState.Play, 0x0B, () => new KeepAlivePacket()
			{
				PacketId = 0x0B
			});
		//	Register(ConnectionState.Play, 0x0c, () => new PlayerPosition());
		//	Register(ConnectionState.Play, 0x0D, () => new PlayerPositionAndLookPacket());
		//	Register(ConnectionState.Play, 0x0E, () => new PlayerLookPacket());

			Register<ClientSettingsPacket>(Direction.Client, ConnectionState.Play, 0x03);
			Register<ClientStatusPacket>(Direction.Client, ConnectionState.Play, 0x02);
			Register<ChatMessagePacket>(Direction.Client, ConnectionState.Play, 0x01);

			Register(Direction.Server, ConnectionState.Play, 0x03, () => new SpawnMob());
			Register(Direction.Server, ConnectionState.Play, 0x05, () => new SpawnPlayerPacket());
			Register(Direction.Server, ConnectionState.Play, 0x0d, () => new ServerDifficultyPacket());
			Register(Direction.Server, ConnectionState.Play, 0x0E, () => new ChatMessagePacket()
			{
				ServerBound = false
			});
			Register(Direction.Server, ConnectionState.Play, 0x19, () => new PluginMessagePacket());
			Register(Direction.Server, ConnectionState.Play, 0x1E, () => new UnloadChunk());
			Register(Direction.Server, ConnectionState.Play, 0x20, () => new KeepAlivePacket());
			Register(Direction.Server, ConnectionState.Play, 0x21, () => new ChunkDataPacket());
			Register(Direction.Server, ConnectionState.Play, 0x24, () => new JoinGamePacket());
			Register(Direction.Server, ConnectionState.Play, 0x3C, () => new HeldItemChangePacket());
			Register(Direction.Server, ConnectionState.Play, 0x2D, () => new PlayerAbilitiesPacket());
			Register(Direction.Server, ConnectionState.Play, 0x2F, () => new PlayerListItemPacket());
			Register(Direction.Server, ConnectionState.Play, 0x31, () => new PlayerPositionAndLookPacket());
			Register(Direction.Server, ConnectionState.Play, 0x49, () => new TimeUpdatePacket());
			Register(Direction.Server, ConnectionState.Play, 0x27, () => new EntityRelativeMove());
			Register(Direction.Server, ConnectionState.Play, 0x28, () => new EntityLookAndRelativeMove());
			Register(Direction.Server, ConnectionState.Play, 0x29, () => new EntityLook());
			Register(Direction.Server, ConnectionState.Play, 0x38, () => new EntityHeadLook());
			Register(Direction.Server, ConnectionState.Play, 0x40, () => new EntityVelocity());
			Register(Direction.Server, ConnectionState.Play, 0x51, () => new EntityPropertiesPacket());
			Register(Direction.Server, ConnectionState.Play, 0x4F, () => new EntityTeleport());
		}
	}
}
