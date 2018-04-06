using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Alex.API.Data;
using Alex.API.Entities;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Entities;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Handshake;
using Alex.Networking.Java.Packets.Login;
using Alex.Networking.Java.Packets.Play;
using Alex.Networking.Java.Util;
using Alex.Networking.Java.Util.Encryption;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using NLog.Fluent;

namespace Alex.Worlds.Java
{
	internal interface IJavaProvider
	{
		void HandleHandshake(Packet packet);
		void HandleStatus(Packet packet);
		void HandleLogin(Packet packet);
		void HandlePlay(Packet packet);
	}
	public class JavaWorldProvider : WorldProvider, IJavaProvider, IChatProvider
	{
		private Alex Alex { get; }
		private JavaClient Client { get; }
		private string Username { get; }
		private string UUID { get; }
		private string AccessToken { get; }

		private IPEndPoint Endpoint;
		private AutoResetEvent _loginCompleteEvent = new AutoResetEvent(false);
		private TcpClient TcpClient;

		private System.Threading.Timer _gameTickTimer;
		public JavaWorldProvider(Alex alex, IPEndPoint endPoint, string username, string uuid, string accessToken)
		{
			Alex = alex;
			Username = username;
			UUID = uuid;
			AccessToken = accessToken;
			Endpoint = endPoint;

			TcpClient = new TcpClient();
			Client = new JavaClient(this, TcpClient.Client);
		}

		private PlayerLocation _lastSentLocation = new PlayerLocation(Vector3.Zero);
		private int _tickSinceLastPositionUpdate = 0;
		private bool _flying = false;

		private void SendPlayerAbilities(Player player)
		{
			int flags = 0;

			if (_flying)
			{
				flags |= 0x01 << flags;
			}

			if (player.CanFly)
			{
				flags |= 0x03 << flags;
			}

			PlayerAbilitiesPacket abilitiesPacket = new PlayerAbilitiesPacket();
			abilitiesPacket.PacketId = 0x13;
			abilitiesPacket.ServerBound = true;

			abilitiesPacket.Flags = (byte) flags;
			abilitiesPacket.FlyingSpeed = (float) player.FlyingSpeed;
			abilitiesPacket.WalkingSpeed = (float)player.MovementSpeed;

			SendPacket(abilitiesPacket);
		}

		private void GameTick(object state)
		{
			if (WorldReceiver == null) return;

			if (_initiated)
			{
				var p = WorldReceiver.GetPlayerEntity();
				if (p != null && p is Player player && Spawned)
				{
					player.IsSpawned = Spawned;

					if (player.IsFlying != _flying)
					{
						_flying = player.IsFlying;
						
						SendPlayerAbilities(player);
					}

					var pos = player.KnownPosition;
					if (player.KnownPosition != _lastSentLocation)
					{
						PlayerPositionAndLookPacketServerBound packet = new PlayerPositionAndLookPacketServerBound();
						packet.Yaw = pos.Yaw;
						packet.Pitch = pos.Pitch;
						packet.X = pos.X;
						packet.Y = pos.Y;
						packet.Z = pos.Z;
						packet.OnGround = pos.OnGround;

						SendPacket(packet);

						_lastSentLocation = pos;

						_tickSinceLastPositionUpdate = 0;
					}
					else if (_tickSinceLastPositionUpdate >= 20)
					{
						PlayerPosition packet = new PlayerPosition();
						packet.FeetY = pos.Y;
						packet.X = pos.X;
						packet.Z = pos.Z;
						packet.OnGround = pos.OnGround;

						SendPacket(packet);
						_lastSentLocation = pos;

						_tickSinceLastPositionUpdate = 0;
					}
					else
					{
						_tickSinceLastPositionUpdate++;
					}
				}
			}
		}

		private Vector3 _spawn = Vector3.Zero;
		public override Vector3 GetSpawnPoint()
		{
			return _spawn;
		}

		protected override void Initiate(out LevelInfo info, out IChatProvider chatProvider)
		{
			info = new LevelInfo();
			chatProvider = this;

			_initiated = true;
			while (_entitySpawnQueue.TryDequeue(out Entity entity))
			{
				base.SpawnEntity(entity.EntityId, entity);
			}

			_entitySpawnQueue = null;
			WorldReceiver?.UpdatePlayerPosition(_lastReceivedLocation);

			_gameTickTimer = new System.Threading.Timer(GameTick, null, 50, 50);
		}

		void IChatProvider.Send(string message)
		{
			SendPacket(new ChatMessagePacket()
			{
				Position = ChatMessagePacket.Chat,
				Message = message,
				ServerBound = true
			});
		}

		private bool _initiated = false;
		private BlockingCollection<ChunkColumn> _generatingHelper = new BlockingCollection<ChunkColumn>();
		public override Task Load(ProgressReport progressReport)
		{
			return Task.Run(() =>
			{
				progressReport(LoadingState.ConnectingToServer, 0);
				Login(Username, UUID, AccessToken, _loginCompleteEvent);
				progressReport(LoadingState.ConnectingToServer, 99);

				_loginCompleteEvent.WaitOne();

				List<ChunkColumn> generatedChunks = new List<ChunkColumn>();
				//using (CachedWorld cached = new CachedWorld(Alex))
				{
					
					int t = Alex.GameSettings.RenderDistance;
					double radiusSquared = Math.Pow(t, 2);

					var target = radiusSquared * 3;

					int count = 0;
					ChunkColumn column = _generatingHelper.Take();
					do
					{
						base.LoadChunk(column, column.X, column.Z, true);
						//ChunkManager.AddChunk(column, new ChunkCoordinates(column.X, column.Z), false);
						generatedChunks.Add(column);

						progressReport(LoadingState.LoadingChunks, (int) Math.Floor((count / target) * 100));
						count++;
					} while (_generatingHelper.TryTake(out column, 1250));

					_generatingHelper = null;
					count = 0;

					/*Parallel.ForEach(generatedChunks, (c) =>
					{
						cached.ChunkManager.UpdateChunk(c);

						lock (_genLock)
						{
							base.LoadChunk(c, c.X, c.Z, false);
						}

						cached.ChunkManager.RemoveChunk(new ChunkCoordinates(c.X, c.Z), false);

						progressReport(LoadingState.GeneratingVertices, (int)Math.Floor((count / target) * 100));

						count++;
					});*/
				}

				progressReport(LoadingState.Spawning, 99);
				SpinWait.SpinUntil(() => Spawned);
				
			});
		}

		public void ChunkReceived(IChunkColumn chunkColumn, int x, int z, bool update)
		{
			if (_generatingHelper != null && !Spawned)
			{
				_generatingHelper.Add((ChunkColumn)chunkColumn);
				return;
			}

			base.LoadChunk(chunkColumn, x, z, update);
		}

		public void ChunkUnloaded(int x, int z)
		{
			base.UnloadChunk(x,z);
		}

		private Queue<Entity> _entitySpawnQueue = new Queue<Entity>();
		public void SpawnMob(int entityId, Guid uuid, EntityType type, PlayerLocation position, Vector3 velocity)
		{
			var entity = type.Create(null);
			if (entity == null) return;
			entity.KnownPosition = position;
			entity.Velocity = velocity;
			entity.EntityId = entityId;
			entity.UUID = new UUID(uuid.ToByteArray());

			if (!_initiated)
			{
				_entitySpawnQueue.Enqueue(entity);
			}
			else
			{
				base.SpawnEntity(entityId, entity);
			}
		}

		private PlayerLocation _lastReceivedLocation = new PlayerLocation();
		public void UpdatePlayerPosition(PlayerLocation location)
		{
			_lastReceivedLocation = location;

			if (_spawn == Vector3.Zero)
			{
				_spawn = location;
			}
			
			WorldReceiver?.UpdatePlayerPosition(location);
		}

		public void UpdateEntityPosition(long entityId, PlayerLocation location, bool relative)
		{
			WorldReceiver?.UpdateEntityPosition(entityId, location,  relative);
		}

		public void UpdateTime(long worldAge, long timeOfDay)
		{ 
			WorldReceiver?.SetTime(timeOfDay);
		}

		private void SendPacket(Packet packet)
		{
			Client.SendPacket(packet);
		}

		void IJavaProvider.HandlePlay(Packet packet)
		{
			if (packet is KeepAlivePacket keepAlive)
			{
				HandleKeepAlivePacket(keepAlive);
			}
			else if (packet is PlayerPositionAndLookPacket playerPos)
			{
				HandlePlayerPositionAndLookPacket(playerPos);
			}
			else if (packet is ChunkDataPacket chunk)
			{
				HandleChunkData(chunk);
			}
			else if (packet is JoinGamePacket joinGame)
			{
				HandleJoinGamePacket(joinGame);
			}
			else if (packet is UnloadChunk unloadChunk)
			{
				HandleUnloadChunk(unloadChunk);
			}
			else if (packet is ChatMessagePacket chatMessage)
			{
				HandleChatMessagePacket(chatMessage);
			}
			else if (packet is TimeUpdatePacket timeUpdate)
			{
				HandleTimeUpdatePacket(timeUpdate);
			}
			else if (packet is PlayerAbilitiesPacket abilitiesPacket)
			{
				HandlePlayerAbilitiesPacket(abilitiesPacket);
			}
			else if (packet is EntityPropertiesPacket entityProperties)
			{
				HandleEntityPropertiesPacket(entityProperties);
			}
			else
			{
				//Log.Warn($"Unhandled packet: 0x{packet.PacketId:x2} - {packet.ToString()}");
			}
		}

		private void HandleEntityPropertiesPacket(EntityPropertiesPacket packet)
		{
			if (packet.EntityId == 0 || packet.EntityId == _entityId)
			{
				if (WorldReceiver.GetPlayerEntity() is Player player)
				{
					foreach (var prop in packet.Properties.Values)
					{
						if (prop.Key.Equals("generic.movementSpeed", StringComparison.InvariantCultureIgnoreCase))
						{
							player.MovementSpeed = prop.Value;
						}
						else if (prop.Key.Equals("generic.flyingSpeed", StringComparison.InvariantCultureIgnoreCase))
						{
							player.FlyingSpeed = prop.Value;
						}

						//TODO: Modifier data
					}
				}
			}
		}

		private void HandlePlayerAbilitiesPacket(PlayerAbilitiesPacket packet)
		{
			var flags = packet.Flags;
			if (WorldReceiver?.GetPlayerEntity() is Player player)
			{
				player.CanFly = flags.IsBitSet(0x03);
				player.Invulnerable = flags.IsBitSet(0x00);

				if (flags.IsBitSet(0x01))
				{
					player.IsFlying = true;
					_flying = true;
				}
				else
				{
					player.IsFlying = false;
					_flying = false;
				}
			}
		}

		private void HandleTimeUpdatePacket(TimeUpdatePacket packet)
		{
			UpdateTime(packet.WorldAge, packet.TimeOfDay);
		}

		private void HandleChatMessagePacket(ChatMessagePacket packet)
		{
			if (ChatObject.TryParse(packet.Message, out ChatObject chat))
			{
				ChatReceiver?.Receive(chat.ToString());
			}
			else
			{
				Log.Warn($"Failed to parse chat object, received json: {packet.Message}");
			}
		}

		private void HandleUnloadChunk(UnloadChunk packet)
		{
			ChunkUnloaded(packet.X, packet.Z);
		}

		private int _entityId = -1;
		private void HandleJoinGamePacket(JoinGamePacket packet)
		{
			_entityId = packet.EntityId;
			if (WorldReceiver?.GetPlayerEntity() is Player player)
			{
				player.EntityId = packet.EntityId;
				player.Gamemode = (Gamemode) packet.Gamemode;
			}
			else
			{
				Log.Warn($"Could not get player entity!");
			}
			//	if (WorldReceiver.WorldReceiver.GetPlayerEntity() is Player player)
			//{
			//	player.EntityId = packet.EntityId;
			//}
		}

		private void HandleChunkData(ChunkDataPacket chunk)
		{
			if (_loginCompleteEvent != null)
			{
				_loginCompleteEvent.Set();
				_loginCompleteEvent = null;
			}

			if (!Spawned)
			{
				ThreadPool.QueueUserWorkItem(o =>
				{
					ChunkColumn result = new ChunkColumn();
					result.IsDirty = true;
					result.X = chunk.ChunkX;
					result.Z = chunk.ChunkZ;
					result.Read(new MinecraftStream(new MemoryStream(chunk.Buffer)), chunk.AvailableSections, chunk.FullChunk);

					ChunkReceived(result, result.X, result.Z, false);
				});
			}
			else
			{
				ChunkColumn result = new ChunkColumn();
				result.IsDirty = true;
				result.X = chunk.ChunkX;
				result.Z = chunk.ChunkZ;
				result.Read(new MinecraftStream(new MemoryStream(chunk.Buffer)), chunk.AvailableSections, chunk.FullChunk);

				ChunkReceived(result, result.X, result.Z, true);
			}
		}

		private void HandleKeepAlivePacket(KeepAlivePacket packet)
		{
			KeepAlivePacket response = new KeepAlivePacket();
			response.KeepAliveid = packet.KeepAliveid;
			response.PacketId = 0x0B;

			SendPacket(response);
		}

		private void HandlePlayerPositionAndLookPacket(PlayerPositionAndLookPacket packet)
		{
			//_spawned = true;
			TeleportConfirm confirmation = new TeleportConfirm();
			confirmation.TeleportId = packet.TeleportId;
			SendPacket(confirmation);

			PlayerPositionAndLookPacketServerBound response = new PlayerPositionAndLookPacketServerBound();
			response.OnGround = false;
			response.Pitch = packet.Pitch;
			response.Yaw = packet.Yaw;
			response.X = packet.X;
			response.Y = packet.Y;
			response.Z = packet.Z;

			SendPacket(response);

			UpdatePlayerPosition(
				new PlayerLocation(packet.X, packet.Y, packet.Z, packet.Yaw, pitch: packet.Pitch));

			if (!Spawned)
			{
				ClientStatusPacket clientStatus = new ClientStatusPacket();
				clientStatus.ActionID = ClientStatusPacket.Action.PerformRespawnOrConfirmLogin;
				SendPacket(clientStatus);

				Spawned = true;
			}
		}

		void IJavaProvider.HandleHandshake(Packet packet)
		{

		}

		void IJavaProvider.HandleStatus(Packet packet)
		{

		}

		void IJavaProvider.HandleLogin(Packet packet)
		{
			if (packet is DisconnectPacket disconnect)
			{
				HandleDisconnectPacket(disconnect);
			}
			else if (packet is EncryptionRequestPacket)
			{
				HandleEncryptionRequest((EncryptionRequestPacket)packet);
			}
			else if (packet is SetCompressionPacket compression)
			{
				HandleSetCompression(compression);
			}
			else if (packet is LoginSuccessPacket success)
			{
				HandleLoginSuccess(success);
			}
			else if (packet is SpawnMob spawnMob)
			{
				HandleSpawnMob(spawnMob);
			}
		}

		private void HandleSpawnMob(SpawnMob packet)
		{
			SpawnMob(packet.EntityId, packet.Uuid, (EntityType)packet.Type, new PlayerLocation(packet.X, packet.Y, packet.Z, packet.Yaw, packet.Yaw, packet.Pitch), new Vector3(packet.VelocityX, packet.VelocityY, packet.VelocityZ));
		}

		private void HandleDisconnectPacket(DisconnectPacket packet)
		{
			Log.Info($"Received disconnect: {packet.Message}");
			Client.Stop();
		}

		public bool Spawned = false;
		private void HandleLoginSuccess(LoginSuccessPacket packet)
		{
			Client.ConnectionState = ConnectionState.Play;

			/*ClientSettingsPacket settings = new ClientSettingsPacket();
			settings.ChatColors = false;
			settings.ChatMode = 0;
			settings.ViewDistance = 12;
			settings.SkinParts = 255;
			settings.MainHand = 1;
			settings.Locale = "en_US";
			SendPacket(settings);*/
		}

		private void HandleSetCompression(SetCompressionPacket packet)
		{
			Client.CompressionThreshold = packet.Threshold;
			Client.CompressionEnabled = true;
		}

		private string _accesToken = "";
		private string _uuid = "";
		private string _username = "";
		private byte[] SharedSecret = new byte[16];
		private void HandleEncryptionRequest(EncryptionRequestPacket packet)
		{
			Random random = new Random();
			random.NextBytes(SharedSecret);

			string serverHash;
			using (MemoryStream ms = new MemoryStream())
			{
				byte[] ascii = Encoding.ASCII.GetBytes(packet.ServerId);
				ms.Write(ascii, 0, ascii.Length);
				ms.Write(SharedSecret, 0, 16);
				ms.Write(packet.PublicKey, 0, packet.PublicKey.Length);

				serverHash = JavaHexDigest(ms.ToArray());
			}

			if (!string.IsNullOrWhiteSpace(_accesToken))
			{
				var baseAddress = "https://sessionserver.mojang.com/session/minecraft/join";

				var http = (HttpWebRequest)WebRequest.Create(new Uri(baseAddress));
				http.Accept = "application/json";
				http.ContentType = "application/json";
				http.Method = "POST";

				var bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new JoinRequest()
				{
					ServerId = serverHash,
					SelectedProfile = _uuid,
					AccessToken = _accesToken
				}));

				using (Stream newStream = http.GetRequestStream())
				{
					newStream.Write(bytes, 0, bytes.Length);
				}

				var r = http.GetResponse();

				using (var stream = r.GetResponseStream())
				using (var sr = new StreamReader(stream))
				{
					var content = sr.ReadToEnd();
				}
			}

			var cryptoProvider = AsnKeyBuilder.DecodePublicKey(packet.PublicKey);
			var encrypted = cryptoProvider.Encrypt(SharedSecret, false);

			EncryptionResponsePacket response = new EncryptionResponsePacket();
			response.SharedSecret = encrypted;
			response.VerifyToken = cryptoProvider.Encrypt(packet.VerifyToken, false);
			SendPacket(response);

			Client.InitEncryption(SharedSecret);
		}


		public void Login(string username, string uuid, string accessToken, AutoResetEvent signalWhenReady)
		{
			_loginCompleteEvent = signalWhenReady;
			_username = username;
			_uuid = uuid;
			_accesToken = accessToken;

			TcpClient.Connect(Endpoint);
		//	Client.InitEncryption();
			Client.Initialize();

			HandshakePacket handshake = new HandshakePacket();
			handshake.NextState = ConnectionState.Login;
			handshake.ServerAddress = Endpoint.Address.ToString();
			handshake.ServerPort = (ushort)Endpoint.Port;
			handshake.ProtocolVersion = 368;
			SendPacket(handshake);

			Client.ConnectionState = ConnectionState.Login;

			LoginStartPacket loginStart = new LoginStartPacket();
			loginStart.Username = _username;
			SendPacket(loginStart);
		}

		public sealed class JoinRequest
		{
			[JsonProperty("accessToken")]
			public string AccessToken;

			[JsonProperty("selectedProfile")]
			public string SelectedProfile;

			[JsonProperty("serverId")]
			public string ServerId;
		}

		private static string JavaHexDigest(byte[] input)
		{
			var sha1 = SHA1.Create();
			byte[] hash = sha1.ComputeHash(input);
			bool negative = (hash[0] & 0x80) == 0x80;
			if (negative) // check for negative hashes
				hash = TwosCompliment(hash);
			// Create the string and trim away the zeroes
			string digest = GetHexString(hash).TrimStart('0');
			if (negative)
				digest = "-" + digest;
			return digest;
		}

		private static string GetHexString(byte[] p)
		{
			string result = string.Empty;
			for (int i = 0; i < p.Length; i++)
				result += p[i].ToString("x2"); // Converts to hex string
			return result;
		}

		private static byte[] TwosCompliment(byte[] p) // little endian
		{
			int i;
			bool carry = true;
			for (i = p.Length - 1; i >= 0; i--)
			{
				p[i] = (byte)~p[i];
				if (carry)
				{
					carry = p[i] == 0xFF;
					p[i]++;
				}
			}
			return p;
		}

		public override void Dispose()
		{
			Spawned = false;
			_initiated = false;
			
			base.Dispose();
			_gameTickTimer?.Dispose();

			Client.Stop();
			TcpClient.Dispose();
		}
	}
}
