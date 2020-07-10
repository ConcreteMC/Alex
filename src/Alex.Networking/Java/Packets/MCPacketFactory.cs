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

		internal static void Register<TPacket>(PacketDirection packetDirection, ConnectionState state, int packetId) where TPacket : Packet, new()
		{
			Register(packetDirection, state, packetId, () => new TPacket());
		}

		internal static void Register<TPacket>(PacketDirection packetDirection, ConnectionState state, int packetId, Func<TPacket> createFunc) where TPacket : Packet
		{
			if (packetDirection == PacketDirection.ServerBound)
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

		public static TPacket CreatePacket<TPacket>(PacketDirection packetDirection, ConnectionState state) where TPacket : Packet
		{
			if (packetDirection == PacketDirection.ServerBound)
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

		public static Packet GetPacket(PacketDirection packetDirection, ConnectionState state, int packetId)
		{
			bool success;
			Packet outPacket;
			if (packetDirection == PacketDirection.ServerBound)
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
			Register(PacketDirection.ServerBound, ConnectionState.Handshake, 0x00, () => new HandshakePacket());
		}

		private static void RegisterStatus()
		{
			Register(PacketDirection.ServerBound, ConnectionState.Status, 0x00, () => new RequestPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Status, 0x00, () => new ResponsePacket());

			Register(PacketDirection.ServerBound, ConnectionState.Status, 0x01, () => new PingPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Status, 0x01, () => new PingPacket());
		}

		private static void RegisterLogin()
		{
			//Register(Direction.ServerBound, ConnectionState.Login, 0x00, () => new LoginPluginMessagePacket());
			Register(PacketDirection.ServerBound, ConnectionState.Login, 0x00, () => new LoginStartPacket());
			Register(PacketDirection.ServerBound, ConnectionState.Login, 0x01, () => new EncryptionResponsePacket());
			Register(PacketDirection.ServerBound, ConnectionState.Login, 0x02, () => new LoginPluginResponsePacket());

			//Register(Direction.ClientBound, ConnectionState.Login, 0x00, () => new LoginPluginMessagePacket());
			Register(PacketDirection.ClientBound, ConnectionState.Login, 0x00, () => new DisconnectPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Login, 0x01, () => new EncryptionRequestPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Login, 0x02, () => new LoginSuccessPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Login, 0x03, () => new SetCompressionPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Login, 0x04, () => new LoginPluginRequestPacket());
		}

		private static void RegisterPlay()
		{
			Register(PacketDirection.ServerBound, ConnectionState.Play, 0x0C, () => new KeepAlivePacket()
			{
				PacketId = 0x0B
			});
			Register<ClientSettingsPacket>(PacketDirection.ServerBound, ConnectionState.Play, 0x03);
			Register<ClientStatusPacket>(PacketDirection.ServerBound, ConnectionState.Play, 0x02);
			Register<ChatMessagePacket>(PacketDirection.ServerBound, ConnectionState.Play, 0x01);

		//	Register(Direction.ClientBound, ConnectionState.Play, 0x03, () => new SpawnMob());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x00, () => new SpawnEntity());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x02, () => new SpawnLivingEntity());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x04, () => new SpawnPlayerPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x05, () => new EntityAnimationPacket());
			
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x0B, () => new BlockChangePacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x0D, () => new ServerDifficultyPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x0E, () => new ChatMessagePacket()
			{
				ServerBound = false
			});
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x0F, () => new MultiBlockChange());
			
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x10, () => new TabCompleteClientBound());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x12, () => new WindowConfirmationPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x13, () => new CloseWindowPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x14, () => new WindowItems());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x16, () => new SetSlot());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x18, () => new PluginMessagePacket());
			
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x1A, () => new DisconnectPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x1B, () => new EntityStatusPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x1E, () => new ChangeGameStatePacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x1D, () => new UnloadChunk());
			
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x20, () => new KeepAlivePacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x21, () => new ChunkDataPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x23, () => new ParticlePacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x24, () => new UpdateLightPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x25, () => new JoinGamePacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x28, () => new EntityRelativeMove());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x29, () => new EntityLookAndRelativeMove());
			
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x2A, () => new EntityLook());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x2E, () => new OpenWindowPacket());
			
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x31, () => new PlayerAbilitiesPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x32, () => new CombatEventPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x33, () => new PlayerListItemPacket()); //< -----
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x35, () => new PlayerPositionAndLookPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x37, () => new DestroyEntitiesPacket());
			
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x3A, () => new RespawnPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x3B, () => new EntityHeadLook());

			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x3F, () => new HeldItemChangePacket());
			
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x40, () => new UpdateViewPositionPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x41, () => new UpdateViewDistancePacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x42, () => new SpawnPositionPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x43, () => new DisplayScoreboardPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x44, () => new EntityMetadataPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x46, () => new EntityVelocity());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x47, () => new EntityEquipmentPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x48, () => new SetExperiencePacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x49, () => new UpdateHealthPacket());
			
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x4A, () => new ScoreboardObjectivePacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x4c, () => new TeamsPacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x4E, () => new TimeUpdatePacket());
			
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x4F, () => new TitlePacket());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x56, () => new EntityTeleport());
			Register(PacketDirection.ClientBound, ConnectionState.Play, 0x58, () => new EntityPropertiesPacket());





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
