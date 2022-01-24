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

		static MCPacketFactory()
		{
			HandshakeFactory = new PacketFactory<int, MinecraftStream, Packet>();
			StatusFactory = new PacketFactory<int, MinecraftStream, Packet>();
			LoginFactory = new PacketFactory<int, MinecraftStream, Packet>();
			PlayFactory = new PacketFactory<int, MinecraftStream, Packet>();
		}

		internal static void Register<TPacket>(ConnectionState state, int packetId) where TPacket : Packet, new()
		{
			Register(state, packetId, () => new TPacket());
		}

		internal static void Register<TPacket>(ConnectionState state, int packetId, Func<TPacket> createFunc)
			where TPacket : Packet
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

		public static TPacket CreatePacket<TPacket>(ConnectionState state) where TPacket : Packet
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

		public static Packet GetPacket(ConnectionState state, int packetId)
		{
			bool success;
			Packet outPacket;

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

		private static void RegisterHandshake() { }

		private static void RegisterStatus()
		{
			Register(ConnectionState.Status, 0x00, () => ResponsePacket.CreateObject());
			Register(ConnectionState.Status, 0x01, () => PongPacket.CreateObject());
		}

		private static void RegisterLogin()
		{
			Register(ConnectionState.Login, 0x00, () => DisconnectPacket.CreateObject());
			Register(ConnectionState.Login, 0x01, () => EncryptionRequestPacket.CreateObject());
			Register(ConnectionState.Login, 0x02, () => LoginSuccessPacket.CreateObject());
			Register(ConnectionState.Login, 0x03, () => SetCompressionPacket.CreateObject());
			Register(ConnectionState.Login, 0x04, () => LoginPluginRequestPacket.CreateObject());
		}

		private static void RegisterPlay()
		{
			Register(ConnectionState.Play, 0x00, () => SpawnEntity.CreateObject());
			Register(ConnectionState.Play, 0x02, () => SpawnLivingEntity.CreateObject());
			Register(ConnectionState.Play, 0x04, () => SpawnPlayerPacket.CreateObject());
			Register(ConnectionState.Play, 0x06, () => EntityAnimationPacket.CreateObject());
			Register(ConnectionState.Play, 0x08, () => AcknowledgePlayerDiggingPacket.CreateObject());
			Register(ConnectionState.Play, 0x09, () => BlockBreakAnimationPacket.CreateObject());
			Register(ConnectionState.Play, 0x0A, () => BlockEntityDataPacket.CreateObject());

			Register(ConnectionState.Play, 0x0B, () => BlockActionPacket.CreateObject());
			Register(ConnectionState.Play, 0x0C, () => BlockChangePacket.CreateObject());
			Register(ConnectionState.Play, 0x0D, () => BossBarPacket.CreateObject());
			Register(ConnectionState.Play, 0x0E, () => ServerDifficultyPacket.CreateObject());

			Register(
				ConnectionState.Play, 0x0F, () =>
				{
					var packet = ChatMessagePacket.CreateObject();
					packet.ServerBound = false;

					return packet;
				});
			//{
			//	ServerBound = false
			//});
			//	Register(ConnectionState.Play, 0x0F, () => MultiBlockChange.CreateObject());

			Register(ConnectionState.Play, 0x11, () => TabCompleteClientBound.CreateObject());
			Register(ConnectionState.Play, 0x12, () => DeclareCommandsPacket.CreateObject());
			//Register(ConnectionState.Play, 0x11, () => WindowConfirmationPacket.CreateObject());
			Register(ConnectionState.Play, 0x13, () => CloseWindowPacket.CreateObject());
			Register(ConnectionState.Play, 0x14, () => WindowItems.CreateObject());
			Register(ConnectionState.Play, 0x16, () => SetSlot.CreateObject());
			Register(ConnectionState.Play, 0x18, () => PluginMessagePacket.CreateObject());
			Register(ConnectionState.Play, 0x19, () => NamedSoundEffectPacket.CreateObject());

			Register(ConnectionState.Play, 0x1A, () => DisconnectPacket.CreateObject());
			Register(ConnectionState.Play, 0x1B, () => EntityStatusPacket.CreateObject());
			Register(ConnectionState.Play, 0x1D, () => UnloadChunk.CreateObject());
			Register(ConnectionState.Play, 0x1E, () => ChangeGameStatePacket.CreateObject());

			Register(ConnectionState.Play, 0x21, () => KeepAlivePacket.CreateObject());
			Register(ConnectionState.Play, 0x22, () => ChunkDataPacket.CreateObject());
			Register(ConnectionState.Play, 0x24, () => ParticlePacket.CreateObject());
			Register(ConnectionState.Play, 0x25, () => UpdateLightPacket.CreateObject());
			Register(ConnectionState.Play, 0x26, () => JoinGamePacket.CreateObject());

			Register(ConnectionState.Play, 0x29, () => EntityRelativeMove.CreateObject());
			Register(ConnectionState.Play, 0x2A, () => EntityLookAndRelativeMove.CreateObject());
			Register(ConnectionState.Play, 0x2B, () => EntityLook.CreateObject());

			Register(ConnectionState.Play, 0x2E, () => OpenWindowPacket.CreateObject());

			Register(ConnectionState.Play, 0x30, () => PlayPingPacket.CreateObject());
			Register(ConnectionState.Play, 0x32, () => PlayerAbilitiesPacket.CreateObject());
			//Register(ConnectionState.Play, 0x31, () => CombatEventPacket.CreateObject());
			Register(ConnectionState.Play, 0x36, () => PlayerListItemPacket.CreateObject()); //< -----
			Register(ConnectionState.Play, 0x37, () => FacePlayerPacket.CreateObject());
			Register(ConnectionState.Play, 0x38, () => PlayerPositionAndLookPacket.CreateObject());
			Register(ConnectionState.Play, 0x3A, () => DestroyEntitiesPacket.CreateObject());
			Register(ConnectionState.Play, 0x3D, () => RespawnPacket.CreateObject());

			Register(ConnectionState.Play, 0x3E, () => EntityHeadLook.CreateObject());
			Register(ConnectionState.Play, 0x3F, () => MultiBlockChange.CreateObject());

			Register(ConnectionState.Play, 0x48, () => HeldItemChangePacket.CreateObject());
			Register(ConnectionState.Play, 0x49, () => UpdateViewPositionPacket.CreateObject());

			Register(ConnectionState.Play, 0x4A, () => UpdateViewDistancePacket.CreateObject());
			Register(ConnectionState.Play, 0x4B, () => SpawnPositionPacket.CreateObject());
			Register(ConnectionState.Play, 0x4C, () => DisplayScoreboardPacket.CreateObject());
			Register(ConnectionState.Play, 0x4D, () => EntityMetadataPacket.CreateObject());
			Register(ConnectionState.Play, 0x4F, () => EntityVelocity.CreateObject());

			Register(ConnectionState.Play, 0x50, () => EntityEquipmentPacket.CreateObject());
			Register(ConnectionState.Play, 0x51, () => SetExperiencePacket.CreateObject());
			Register(ConnectionState.Play, 0x52, () => UpdateHealthPacket.CreateObject());

			Register(ConnectionState.Play, 0x53, () => ScoreboardObjectivePacket.CreateObject());
			Register(ConnectionState.Play, 0x55, () => TeamsPacket.CreateObject());
			Register(ConnectionState.Play, 0x56, () => UpdateScorePacket.CreateObject());
			Register(ConnectionState.Play, 0x58, () => SetTitleSubTextPacket.CreateObject());
			Register(ConnectionState.Play, 0x59, () => TimeUpdatePacket.CreateObject());

			Register(ConnectionState.Play, 0x5A, () => SetTitleTextPacket.CreateObject());
			Register(ConnectionState.Play, 0x5B, () => SetTitleTimesPacket.CreateObject());

			Register(ConnectionState.Play, 0x5C, () => EntitySoundEffectPacket.CreateObject());
			Register(ConnectionState.Play, 0x5D, () => SoundEffectPacket.CreateObject());

			Register(ConnectionState.Play, 0x62, () => EntityTeleport.CreateObject());
			Register(ConnectionState.Play, 0x64, () => EntityPropertiesPacket.CreateObject());
			Register(ConnectionState.Play, 0x65, () => EntityEffectPacket.CreateObject());
		}

		public static string GetPlayPacketName(int id)
		{
			return "Unknown";
		}
	}
}