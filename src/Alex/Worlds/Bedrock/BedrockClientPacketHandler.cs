using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Blocks.State;
using Alex.API.Entities;
using Alex.API.Events;
using Alex.API.Events.World;
using Alex.API.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Entities;
using Alex.GameStates;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Geometry;
using Alex.Items;
using Alex.Networking.Bedrock;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using fNbt;
using Jose;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MiNET.Net;
using MiNET.UI;
using MiNET.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using BitArray = System.Collections.BitArray;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using BlockState = MiNET.Utils.IBlockState;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using Inventory = Alex.Utils.Inventory;
using MathF = System.MathF;
using NibbleArray = MiNET.Utils.NibbleArray;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Worlds.Bedrock
{
	public class BedrockClientPacketHandler : IMcpeClientMessageHandler
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockClientPacketHandler));

		private BedrockClient Client { get; }
		private Alex AlexInstance { get; }
        private CancellationToken CancellationToken { get; }
        private ChunkProcessor ChunkProcessor { get; }

        private WorldProvider WorldProvider { get; }
        private PlayerProfile PlayerProfile { get; }
        private IEventDispatcher EventDispatcher { get; }
        private IStorageSystem Storage { get; }
        private bool UseCustomEntityModels { get; set; }
        public BedrockClientPacketHandler(BedrockClient client, IEventDispatcher eventDispatcher, WorldProvider worldProvider, PlayerProfile profile, Alex alex, CancellationToken cancellationToken, ChunkProcessor chunkProcessor) //:
	       // base(client)
        {
	        Storage = alex.Services.GetRequiredService<IStorageSystem>();
	        
	        EventDispatcher = eventDispatcher;
	        Client = client;
	        AlexInstance = alex;
	        CancellationToken = cancellationToken;
	        WorldProvider = worldProvider;
	        PlayerProfile = profile;
	        
	        AnvilWorldProvider.LoadBlockConverter();

	        ChunkProcessor = chunkProcessor;

	        var options = alex.Services.GetRequiredService<IOptionsProvider>().AlexOptions;
	        options.VideoOptions.CustomSkins.Bind((value, newValue) => UseCustomEntityModels = newValue);
	        UseCustomEntityModels = options.VideoOptions.CustomSkins.Value;
        }

        public bool ReportUnhandled { get; set; } = false;
        private void UnhandledPackage(Packet packet)
		{
			if (ReportUnhandled)
				Log.Warn($"Unhandled bedrock packet: {packet.GetType().Name} (0x{packet.Id:X2})");
		}

        public void HandleMcpeServerToClientHandshake(McpeServerToClientHandshake message)
        {
	        string token = message.token;

	        IDictionary<string, dynamic> headers = JWT.Headers(token);
	        string x5u = headers["x5u"];

	        ECPublicKeyParameters remotePublicKey = (ECPublicKeyParameters) PublicKeyFactory.CreateKey(x5u.DecodeBase64Url());

	        var signParam = new ECParameters
	        {
		        Curve = ECCurve.NamedCurves.nistP384,
		        Q =
		        {
			        X = remotePublicKey.Q.AffineXCoord.GetEncoded(),
			        Y = remotePublicKey.Q.AffineYCoord.GetEncoded()
		        },
	        };
	        signParam.Validate();

	        var signKey = ECDsa.Create(signParam);

	        try
	        {
		        var data = JWT.Decode<HandshakeData>(token, signKey);
				//var data = JWT.Payload<HandshakeData>(token);
		        Client.InitiateEncryption(Base64Url.Decode(x5u), Base64Url.Decode(data.salt));
	        }
	        catch (Exception e)
	        {
		        //AlexInstance.GameStateManager.Back();
		        string msg = $"Network error.";

		        if (e is Jose.IntegrityException)
		        {
			        msg = $"Invalid server signature!";
		        }
		        
		        Client.ShowDisconnect(msg);
		        
		        Log.Error(e, $"Could complete handshake: {e.ToString()}");
		        throw;
	        }
        }

        public void HandleMcpePlayStatus(McpePlayStatus message)
		{
			Log.Info($"Client status: {message.status}");
			Client.PlayerStatus = message.status;
			
			if (Client.PlayerStatus == 3)
			{
				Client.HasSpawned = true;

				Client.PlayerStatusChanged.Set();

				//if (Client is Bedrockcl miNetClient)
				//{
					//miNetClient.IsEmulator = false;
				//}

				Client.WorldReceiver.Player.EntityId = Client.EntityId;
			}
		}

        public void HandleMcpeDisconnect(McpeDisconnect message)
        {
            Log.Info($"Received disconnect: {message.message}");
            Client.ShowDisconnect(message.message, false);
            
           // Client.
           // base.HandleMcpeDisconnect(message);
        }

        public void HandleMcpeResourcePackDataInfo(McpeResourcePackDataInfo message)
        {
	        McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
	        response.responseStatus = 3;
	        Client.SendPacket(response);
        }

        public void HandleMcpeResourcePackChunkData(McpeResourcePackChunkData message)
        {
	        throw new NotImplementedException();
        }
        
        public void HandleMcpeResourcePacksInfo(McpeResourcePacksInfo message)
        {
	        if (message.resourcepackinfos.Count != 0)
	        {
		        ResourcePackIds resourcePackIds = new ResourcePackIds();

		        foreach (var packInfo in message.resourcepackinfos)
		        {
			        resourcePackIds.Add(packInfo.PackIdVersion.Id);
		        }

		        McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
		        response.responseStatus = 2;
		        response.resourcepackids = resourcePackIds;
		        Client.SendPacket(response);
	        }
	        else
	        {
		        McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
		        response.responseStatus = 3;
		        Client.SendPacket(response);
	        }
        }

        public void HandleMcpeResourcePackStack(McpeResourcePackStack message)
        {
	        McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
	        response.responseStatus = 4;
	        Client.SendPacket(response);
        }

        public void HandleMcpeText(McpeText message)
		{
			EventDispatcher.DispatchEvent(new ChatMessageReceivedEvent(new ChatObject(message.message), (MessageType)message.type));
		}

		public void HandleMcpeSetTime(McpeSetTime message)
		{
			Client.WorldReceiver?.SetTime(message.time);
			
			_changeDimensionResetEvent.Set();
		}

		public void HandleMcpeStartGame(McpeStartGame message)
		{
			Client.EntityId = message.runtimeEntityId;
			Client.NetworkEntityId = message.entityIdSelf;
			Client.SpawnPoint = message.spawn;
			//Client.CurrentLocation = new MiNET.Utils.PlayerLocation(Client.SpawnPoint, message.spawn.X, message.spawn.X, message.spawn.Y);

			Client.WorldReceiver?.UpdatePlayerPosition(
				new API.Utils.PlayerLocation(
					new Microsoft.Xna.Framework.Vector3(Client.SpawnPoint.X, Client.SpawnPoint.Y, Client.SpawnPoint.Z),
					message.spawn.X, message.spawn.X, message.spawn.Y));

			//message.BlockPalette
			//File.WriteAllText("states.json", JsonConvert.SerializeObject(message.blockPallet));

			Dictionary<uint, BlockStateContainer> ourStates = new Dictionary<uint, BlockStateContainer>();

			foreach (var bs in message.blockPalette)
			{
				foreach (var blockstate in bs.States)
				{
					var name = blockstate.Name;

					if (name != null)
					{
						if (name.Equals("minecraft:grass", StringComparison.InvariantCultureIgnoreCase))
							name = "minecraft:grass_block";

						blockstate.Name = name;
					}

					//var state = BlockFactory.GetBlockState(name);
				}

				var name2 = bs.Name;

				if (name2 != null)
				{
					if (name2.Equals("minecraft:grass", StringComparison.InvariantCultureIgnoreCase))
						name2 = "minecraft:grass_block";

					bs.Name = name2;
				}

				ourStates.TryAdd((uint) bs.RuntimeId, bs);
			}

			ChunkProcessor._blockStateMap = ourStates;
			Client.RequestChunkRadius(Client.ChunkRadius);
			
			Client.WorldReceiver.Player.EntityId = message.runtimeEntityId;
			Client.WorldReceiver.Player.UpdateGamemode((Gamemode) message.playerGamemode);

			foreach (var gr in message.gamerules)
			{
				Client.WorldReceiver.SetGameRule(gr);
			}

			Client.WorldReceiver.Player.Inventory.IsPeInventory = true;

			_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
		}

		public void HandleMcpeMovePlayer(McpeMovePlayer message)
		{
			if (message.runtimeEntityId != Client.EntityId)
			{
				Client.WorldReceiver.UpdateEntityPosition(message.runtimeEntityId, 
					new PlayerLocation(message.x, message.y - Player.EyeLevel, message.z, message.headYaw, message.yaw, -message.pitch));
				return;
			}
			
			Client.WorldReceiver.UpdatePlayerPosition(new 
				PlayerLocation(message.x, message.y, message.z));

			// Client.SendMcpeMovePlayer();
		//	Client.CurrentLocation = new MiNET.Utils.PlayerLocation(message.x, message.y, message.z);
			Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(message.x, message.y, message.z), false);
			
			_changeDimensionResetEvent.Set();
		}


		public void HandleMcpeAdventureSettings(McpeAdventureSettings message)
		{
			//Client.UserPermission = (CommandPermission) message.commandPermission;
			
			//base.HandleMcpeAdventureSettings(message);
			if (Client.WorldReceiver.Player is Player player)
			{
				player.CanFly = ((message.flags & 0x40) == 0x40);
				player.IsFlying = ((message.flags & 0x200) == 0x200);
				player.IsWorldImmutable = ((message.flags & 0x01) == 0x01);
				player.IsNoPvP = (message.flags & 0x02) == 0x02;
				player.IsNoPvM = (message.flags & 0x04) == 0x04;
			}
		}

		private ConcurrentDictionary<MiNET.Utils.UUID, PlayerMob> _players = new ConcurrentDictionary<MiNET.Utils.UUID, PlayerMob>();
        public void HandleMcpeAddPlayer(McpeAddPlayer message)
		{
		//	UUID u = new UUID(message.uuid.GetBytes());
			if (_players.TryGetValue(message.uuid, out PlayerMob mob))
			{
				//MiNET.Player
				mob.EntityId = message.runtimeEntityId;
				mob.KnownPosition = new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch);
				mob.Velocity = new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ) * 20f;
				
				mob.HandleMetadata(message.metadata);
				//mob.Height = message.metadata[(int) MiNET.Entities.Entity.MetadataFlags.CollisionBoxHeight]
				
				if (Client.WorldReceiver is World w)
				{
					mob.IsSpawned = true;
					
					_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
					w.SpawnEntity(mob.EntityId, mob);
				}
				else
				{
					mob.IsSpawned = false;
				}
			}
			else
			{
				Log.Warn($"Tried spawning invalid player: {message.uuid}");
			}
		}
        
		public void HandleMcpePlayerList(McpePlayerList message)
		{
			if (message.records is PlayerAddRecords addRecords)
			{
				foreach (var r in addRecords)
				{
					var u = new API.Utils.UUID(r.ClientUuid.GetBytes());
					if (_players.ContainsKey(r.ClientUuid)) continue;

					bool isTransparent = false;
					PooledTexture2D skinTexture;
					Image<Rgba32> skinBitmap;
					if (r.Skin.TryGetBitmap(out skinBitmap))
					{
						skinTexture =
							TextureUtils.BitmapToTexture2D(AlexInstance.GraphicsDevice, skinBitmap);
					}
					else
					{
						AlexInstance.Resources.ResourcePack.TryGetBitmap("entity/alex", out var rawTexture);
						skinTexture = TextureUtils.BitmapToTexture2D(AlexInstance.GraphicsDevice, rawTexture);
					}

					Client.WorldReceiver?.AddPlayerListItem(new PlayerListItem(u, r.DisplayName, Gamemode.Survival, 0));

					EntityModelRenderer renderer = null;
					if (UseCustomEntityModels)
					{
						if (!string.IsNullOrWhiteSpace(r.Skin.GeometryData) && r.Skin.GeometryData != "null")
						{
							try
							{
								MinecraftGeometry geometry = null;
								var               jObject  = JObject.Parse(r.Skin.GeometryData);

								if (jObject.TryGetValue(
									r.Skin.SkinResourcePatch.Geometry.Default,
									StringComparison.InvariantCultureIgnoreCase, out JToken value))
								{
									geometry = value.ToObject<MinecraftGeometry>(
										JsonSerializer.Create(
											new JsonSerializerSettings()
											{
												Converters = new List<JsonConverter>()
												{
													new SingleOrArrayConverter<Vector3>(),
													new SingleOrArrayConverter<Vector2>(),
													new Vector3Converter(),
													new Vector2Converter()
												}
											}));
								}
								else
								{
									foreach (var prop in jObject)
									{
										if (prop.Key.StartsWith(r.Skin.SkinResourcePatch.Geometry.Default))
										{
											var split = prop.Key.Split(':');

											MinecraftGeometry parentGeometry = null;

											if (split.Length > 1)
											{
												if (jObject.TryGetValue(
													split[1], StringComparison.InvariantCultureIgnoreCase,
													out JToken parent))
												{
													parentGeometry = parent.ToObject<MinecraftGeometry>(
														JsonSerializer.Create(
															new JsonSerializerSettings()
															{
																Converters = new List<JsonConverter>()
																{
																	new SingleOrArrayConverter<Vector3>(),
																	new SingleOrArrayConverter<Vector2>(),
																	new Vector3Converter(),
																	new Vector2Converter()
																}
															}));
												}
											}

											//TODO: Support inheritance

											geometry = prop.Value.ToObject<MinecraftGeometry>(
												JsonSerializer.Create(
													new JsonSerializerSettings()
													{
														Converters = new List<JsonConverter>()
														{
															new SingleOrArrayConverter<Vector3>(),
															new SingleOrArrayConverter<Vector2>(),
															new Vector3Converter(),
															new Vector2Converter()
														}
													}));

											if (parentGeometry != null)
											{
												foreach (var bone in parentGeometry.Bones)
												{
													var childBone =
														geometry.Bones.FirstOrDefault(x => x.Name == bone.Name);

													if (childBone == null)
													{
														geometry.Bones.Add(bone);
													}
													else
													{
														if (childBone.NeverRender)
															continue;

													}
												}
											}

											break;
										}
									}
								}

								if (geometry == null)
								{
									if (r.Skin.GeometryData.Contains("\"format_version\""))
									{
										BedrockGeometry geo = JsonConvert.DeserializeObject<BedrockGeometry>(
											r.Skin.GeometryData, new SingleOrArrayConverter<Vector3>(),
											new SingleOrArrayConverter<Vector2>(), new Vector3Converter(),
											new Vector2Converter());

										if (geo != null)
										{
											geometry = geo.MinecraftGeometry.FirstOrDefault();
										}
									}
									else
									{
										Dictionary<string, MinecraftGeometry> geo =
											JsonConvert.DeserializeObject<Dictionary<string, MinecraftGeometry>>(
												r.Skin.GeometryData, new SingleOrArrayConverter<Vector3>(),
												new SingleOrArrayConverter<Vector2>(), new Vector3Converter(),
												new Vector2Converter());

										if (geo.TryGetValue(
											r.Skin.SkinResourcePatch.Geometry.Default,
											out MinecraftGeometry minecraftGeometry))
										{
											geometry = minecraftGeometry;
										}
									}
								}

								if (geometry != null)
								{
									if (geometry.Description == null)
									{
										geometry.Description = new SkinDescription()
										{
											Identifier = r.Skin.SkinResourcePatch.Geometry.Default,
											TextureHeight = r.Skin.Height,
											TextureWidth = r.Skin.Width
										};
									}

									//var abc = geo.MinecraftGeometry.FirstOrDefault()
									var modelRenderer = new EntityModelRenderer(geometry, skinTexture);

									if (modelRenderer.Valid)
									{
										renderer = modelRenderer;
									}
									else
									{
										modelRenderer.Dispose();
									}


									//m.ModelRenderer =
									//	new EntityModelRenderer(geo.MinecraftGeometry.FirstOrDefault(), skinTexture);
								}
								else
								{
									Log.Warn($"INVALID SKIN: {r.Skin.SkinResourcePatch.Geometry.Default}");
								}
							}
							catch (Exception ex)
							{
								string name = "N/A";
								/*if (r.Skin.SkinResourcePatch != null)
								{
									name = r.Skin.SkinResourcePatch.Geometry.Default;
								}*/
								Log.Warn(ex, $"Could not create geometry ({name}): {ex.ToString()}");
							}
						}
					}

					PlayerMob m = new PlayerMob(r.DisplayName, Client.WorldReceiver as World, Client, skinTexture);
					m.UUID = u;
					m.EntityId = r.EntityId;
					if (renderer != null)
						m.ModelRenderer = renderer;
					
					if (!_players.TryAdd(r.ClientUuid, m))
					{
						Log.Warn($"Duplicate player record! {r.ClientUuid}");
					}
				}
			}
            else if (message.records is PlayerRemoveRecords removeRecords)
            {
	            foreach (var r in removeRecords)
	            {
		           // var u = new UUID(r.ClientUuid.GetBytes());
		            if (_players.TryRemove(r.ClientUuid, out var player))
		            {
			            Client.WorldReceiver?.RemovePlayerListItem(player.UUID);
			            if (Client.WorldReceiver is World w)
			            {
				            //w.DespawnEntity(player.EntityId);
			            }
		            }
	            }
            }
		}

		public void SpawnMob(long entityId, Guid uuid, EntityType type, PlayerLocation position, Microsoft.Xna.Framework.Vector3 velocity)
		{
			Entity entity = null;
			if (EntityFactory.ModelByType(type, out var renderer, out EntityData knownData))
			{
				//if (Enum.TryParse(knownData.Name, out type))
				//{
				//	entity = type.Create(null);
				//}
                entity = type.Create(null);

				if (entity == null)
				{
					entity = new Entity((int) type, null, Client);
				}

				//if (knownData.Height)
				{
					entity.Height = knownData.Height;
				}

				//if (knownData.Width.HasValue)
				entity.Width = knownData.Width;

				if (string.IsNullOrWhiteSpace(entity.NameTag) && !string.IsNullOrWhiteSpace(knownData.Name))
				{
					entity.NameTag = knownData.Name;
				}
			}

			if (entity == null)
			{
				Log.Warn($"Could not create entity of type: {(int) type}:{type.ToString()}");
				return;
			}

			if (renderer == null)
			{
				var def = AlexInstance.Resources.BedrockResourcePack.EntityDefinitions.FirstOrDefault(x =>
					x.Value.Filename.Replace("_", "").Equals(type.ToString().ToLowerInvariant()));
				if (!string.IsNullOrWhiteSpace(def.Key))
				{
					EntityModel model;
					if (ModelFactory.TryGetModel(def.Value.Geometry["default"],
						    out model) && model != null)
					{
						var textures = def.Value.Textures;
						string texture;
						if (!textures.TryGetValue("default", out texture))
						{
							texture = textures.FirstOrDefault().Value;
						}

						if (AlexInstance.Resources.BedrockResourcePack.Textures.TryGetValue(texture,
							out var bmp))
						{
							PooledTexture2D t = TextureUtils.BitmapToTexture2D(AlexInstance.GraphicsDevice, bmp);

							renderer = new EntityModelRenderer(model, t);
						}
					}
				}
			}

			if (renderer == null)
			{
				Log.Debug($"Missing renderer for entity: {type.ToString()} ({(int) type})");
				return;
			}

			if (renderer.Texture == null)
			{
				Log.Debug($"Missing texture for entity: {type.ToString()} ({(int) type})");
				return;
			}

			entity.ModelRenderer = renderer;

			entity.KnownPosition = position;
			entity.Velocity = velocity;
			entity.EntityId = entityId;
			entity.UUID = new UUID(uuid.ToByteArray());

			Client.WorldReceiver.SpawnEntity(entityId, entity);
		}


		public void HandleMcpeAddEntity(McpeAddEntity message)
		{
			var type = message.entityType.Replace("minecraft:", "");
            if (Enum.TryParse(typeof(EntityType), type, true, out object res))
            {
                SpawnMob(message.runtimeEntityId, Guid.NewGuid(), (EntityType)res, new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch), new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ) * 20f);
                _entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
            }
            else
            {
                Log.Warn($"Unknown mob: {type}");
            }
        }

		private ConcurrentDictionary<long, long> _entityMapping = new ConcurrentDictionary<long, long>();
		public void HandleMcpeRemoveEntity(McpeRemoveEntity message)
		{
			if (_entityMapping.TryRemove(message.entityIdSelf, out var entityId))
			{
				Client.WorldReceiver.DespawnEntity(entityId);
			}
		}

		public void HandleMcpeAddItemEntity(McpeAddItemEntity message)
		{
			var slot = message.item;
			if (ItemFactory.TryGetItem(slot.Id, slot.Metadata, out Item item))
			{
				item.Count = slot.Count;
				item.Nbt = slot.ExtraData;

				ItemEntity itemEntity = new ItemEntity(null, Client);
				itemEntity.EntityId = message.runtimeEntityId;
				itemEntity.Velocity = new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ) * 20f;
				
				itemEntity.SetItem(item);
			
				Client.WorldReceiver.SpawnEntity(message.runtimeEntityId, itemEntity);
				_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
                    
				// Log.Info($"Set inventory slot: {usedIndex} Id: {slot.Id}:{slot.Metadata} x {slot.Count} Name: {item.DisplayName} IsPeInv: {inventory.IsPeInventory}");
			}
		}

		public void HandleMcpeTakeItemEntity(McpeTakeItemEntity message)
		{
			UnhandledPackage(message);
			return;
		}

		public void HandleMcpeMoveEntity(McpeMoveEntity message)
		{
			var location = new PlayerLocation(
				message.position.X, message.position.Y, message.position.Z, message.position.HeadYaw,
				message.position.Yaw, message.position.Pitch);

			if (message.runtimeEntityId != Client.EntityId)
			{
				Client.WorldReceiver.UpdateEntityPosition(message.runtimeEntityId, location);

				return;
			}

			Client.WorldReceiver.UpdatePlayerPosition(location);
		}

		public void HandleMcpeMoveEntityDelta(McpeMoveEntityDelta message)
		{
			//message.GetCurrentPosition(new MiNET.Utils.PlayerLocation(Vector3.Zero));
			if (message.runtimeEntityId != Client.EntityId)
			{
				//TODO: Fix delta reading on packets.

				bool updatePitch = (message.flags & McpeMoveEntityDelta.HasRotX) != 0;
				bool updateYaw = (message.flags & McpeMoveEntityDelta.HasY) != 0;
				bool updateHeadYaw = (message.flags & McpeMoveEntityDelta.HasZ) != 0;
				
				bool updateLook = (updateHeadYaw || updateYaw || updatePitch);

				if (message is EntityDelta ed)
				{

					if (Client.WorldReceiver.TryGetEntity(message.runtimeEntityId, out var entity))
					{
						var before = entity.KnownPosition;
						var known = entity.KnownPosition;
						known = new PlayerLocation(known.X, known.Y, known.Z, known.HeadYaw, known.Yaw, known.Pitch);
						
						var endPosition = ed.GetCurrentPosition(known);

						if (!ed.HasX)
						{
							endPosition.X = known.X;
						}
						else
						{
							endPosition.X = float.IsNaN(endPosition.X) ? known.X : endPosition.X;
						}

						if (!ed.HasY)
						{
							endPosition.Y = known.Y;
						}
						else
						{
							endPosition.Y = float.IsNaN(endPosition.Y) ? known.Y : endPosition.Y;
						}

						if (!ed.HasZ)
						{
							endPosition.Z = known.Z;
						}
						else
						{
							endPosition.Z = float.IsNaN(endPosition.Z) ? known.Z : endPosition.Z;
						}

						endPosition.Yaw = ed.HasYaw ? endPosition.Yaw : known.Yaw;
						endPosition.HeadYaw = ed.HasHeadYaw ? endPosition.HeadYaw : known.HeadYaw;
						endPosition.Pitch = ed.HasPitch ? -endPosition.Pitch : known.Pitch;

						//	Log.Info($"Distance: {endPosition.DistanceTo(known)} | Delta: {(endPosition - known.ToVector3())} | Start: {known.ToVector3()} | End: {endPosition.ToVector3()}");

						entity.KnownPosition = endPosition;
						
						entity.DistanceMoved += MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(before.ToVector3(), endPosition.ToVector3()));
					}
				}
			}
		}

		public void HandleMcpeRiderJump(McpeRiderJump message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateBlock(McpeUpdateBlock message)
		{
			var converted = ChunkProcessor.GetBlockState(message.blockRuntimeId);

			if (converted != null)
			{
				Client.WorldReceiver?.SetBlockState(
					new BlockCoordinates(message.coordinates.X, message.coordinates.Y, message.coordinates.Z), 
					converted, (int) message.storage);
			}
			else
			{
				Log.Warn($"Received unknown block runtime id.");
			}
		}

		public void HandleMcpeAddPainting(McpeAddPainting message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeTickSync(McpeTickSync message)
		{
			UnhandledPackage(message);
		}

		/*public void HandleMcpeExplode(McpeExplode message)
		{
			UnhandledPackage(message);
		}*/

		public void HandleMcpeLevelSoundEventOld(McpeLevelSoundEventOld message)
		{
			UnhandledPackage(message);
        }

		public void HandleMcpeSpawnParticleEffect(McpeSpawnParticleEffect message)
		{
			UnhandledPackage(message);
        }

		public void HandleMcpeAvailableEntityIdentifiers(McpeAvailableEntityIdentifiers message)
		{
			UnhandledPackage(message);
        }

		public void HandleMcpeLevelSoundEventV2(McpeLevelSoundEventV2 message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeNetworkSettingsPacket(McpeNetworkSettingsPacket message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeNetworkChunkPublisherUpdate(McpeNetworkChunkPublisherUpdate message)
		{
			((BedrockClient)Client).LastChunkPublish = message;
			//UnhandledPackage(message);
			//Log.Info($"Chunk publisher update: {message.coordinates} | {message.radius}");
		}

		public void HandleMcpeBiomeDefinitionList(McpeBiomeDefinitionList message)
		{
			UnhandledPackage(message);
        }

		public void HandleMcpeLevelSoundEvent(McpeLevelSoundEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeLevelEventGeneric(McpeLevelEventGeneric message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeLecternUpdate(McpeLecternUpdate message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeVideoStreamConnect(McpeVideoStreamConnect message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeClientCacheStatus(MiNET.Net.McpeClientCacheStatus message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeOnScreenTextureAnimation(McpeOnScreenTextureAnimation message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMapCreateLockedCopy(McpeMapCreateLockedCopy message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeStructureTemplateDataExportRequest(McpeStructureTemplateDataExportRequest message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeStructureTemplateDataExportResponse(McpeStructureTemplateDataExportResponse message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateBlockProperties(McpeUpdateBlockProperties message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeClientCacheBlobStatus(McpeClientCacheBlobStatus message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeClientCacheMissResponse(McpeClientCacheMissResponse message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeLevelEvent(McpeLevelEvent message)
		{
			if (!Client.WorldReceiver.Player.IsBreakingBlock)
				return;

			if ((BlockCoordinates) new Microsoft.Xna.Framework.Vector3(
				message.position.X, message.position.Y, message.position.Z) == Client.WorldReceiver.Player.TargetBlock)
			{
				if (message.eventId == 3600)
				{
					var ticksRequired = (double) ushort.MaxValue / message.data;
					Client.WorldReceiver.Player.BreakTimeNeeded = ticksRequired;
				}
			}
		}

		public void HandleMcpeBlockEvent(McpeBlockEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeEntityEvent(McpeEntityEvent message)
		{
			if (Client.WorldReceiver.TryGetEntity(message.runtimeEntityId, out Entity entity))
			{
				if (message.eventId == 2)
				{
					entity.EntityHurt();
				}
			}
			//UnhandledPackage(message);
		}

		public void HandleMcpeMobEffect(McpeMobEffect message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateAttributes(McpeUpdateAttributes message)
		{
			var player = Client.WorldReceiver.Player;
			if (player.EntityId == message.runtimeEntityId)
			{
				if (message.attributes.TryGetValue("minecraft:health", out var value))
				{
					player.MaxHealth = (int) value.MaxValue;
					player.Health = (int) value.Value;
				}

				if (message.attributes.TryGetValue("minecraft:movement", out var movement))
				{
				//	player.MovementSpeed = movement.Value;
				}

				if (message.attributes.TryGetValue("minecraft:player.hunger", out var hunger))
				{
					player.Hunger = (int) hunger.Value;
					player.MaxHunger = (int) hunger.MaxValue;
				}
				
				if (message.attributes.TryGetValue("minecraft:player.exhaustion", out var exhaustion))
				{
					player.Exhaustion = (int) exhaustion.Value;
					player.MaxExhaustion = (int) exhaustion.MaxValue;
				}
				
				if (message.attributes.TryGetValue("minecraft:player.saturation", out var saturation))
				{
					player.Saturation = (int) saturation.Value;
					player.MaxSaturation = (int) saturation.MaxValue;
				}
			}
		}

		public void HandleMcpeInventoryTransaction(McpeInventoryTransaction message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMobEquipment(McpeMobEquipment message)
		{
			if (Client.WorldReceiver.Player.EntityId == message.runtimeEntityId)
			{
				Client.WorldReceiver.Player.Inventory.SelectedSlot = message.selectedSlot;
				return;
			}
			
			if (Client.WorldReceiver.TryGetEntity(message.runtimeEntityId, out var entity))
			{
				var item = ToAlexItem(message.item);

				byte slot = message.slot;
				switch (message.windowsId)
				{
					case 0:
						
						break;
				}
				
				entity.Inventory[slot] = item;
				entity.Inventory.SelectedSlot = message.selectedSlot;
			}

		//	UnhandledPackage(message);
		}

		public void HandleMcpeMobArmorEquipment(McpeMobArmorEquipment message)
		{
			Entity entity;

			if (!Client.WorldReceiver.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (Client.WorldReceiver.Player.EntityId == message.runtimeEntityId)
				{
					entity = Client.WorldReceiver.Player;
				}

				//entity.Inventory.Boots
			}


			entity.Inventory.Helmet = ToAlexItem(message.helmet);
			entity.Inventory.Chestplate = ToAlexItem(message.chestplate);
			entity.Inventory.Leggings = ToAlexItem(message.leggings);
			entity.Inventory.Boots = ToAlexItem(message.boots);


			//UnhandledPackage(message);
		}

		public void HandleMcpeInteract(McpeInteract message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeHurtArmor(McpeHurtArmor message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetEntityData(McpeSetEntityData message)
		{
			Entity entity = null;
			if (!Client.WorldReceiver.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (Client.WorldReceiver.Player.EntityId == message.runtimeEntityId)
				{
					entity = Client.WorldReceiver.Player;
				}
			}

			if (entity == null)
				return;
			
			entity.HandleMetadata(message.metadata);
			//UnhandledPackage(message);
		}

		public void HandleMcpeSetEntityMotion(McpeSetEntityMotion message)
		{
			var v = message.velocity;
			var velocity = new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z) * 20f;

			Entity entity = null;
			if (!Client.WorldReceiver.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (Client.WorldReceiver.Player.EntityId == message.runtimeEntityId)
				{
					entity = Client.WorldReceiver.Player;
				}
			}

			if (entity == null)
				return;
			
			var old = entity.Velocity;
			entity.Velocity += new Microsoft.Xna.Framework.Vector3(velocity.X - old.X, velocity.Y - old.Y, velocity.Z - old.Z);

			//UnhandledPackage(message);
		}

		public void HandleMcpeSetEntityLink(McpeSetEntityLink message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetHealth(McpeSetHealth message)
		{
			Client.WorldReceiver.Player.Health = message.health;
		}

		public void HandleMcpeSetSpawnPosition(McpeSetSpawnPosition message)
		{
			Client.SpawnPoint = new Vector3(
				message.coordinates.X, (float) (message.coordinates.Y), message.coordinates.Z);

			Client.LevelInfo.SpawnX = (int) Client.SpawnPoint.X;
			Client.LevelInfo.SpawnY = (int) Client.SpawnPoint.Y;
			Client.LevelInfo.SpawnZ = (int) Client.SpawnPoint.Z;

			Client.WorldReceiver.SpawnPoint = new Microsoft.Xna.Framework.Vector3(
				Client.SpawnPoint.X, Client.SpawnPoint.Y, Client.SpawnPoint.Z);
		}

		public void HandleMcpeAnimate(McpeAnimate message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeRespawn(McpeRespawn message)
		{
			Client.WorldReceiver.UpdatePlayerPosition(new PlayerLocation(message.x, message.y, message.z));

			if (Client.PlayerStatus == 3)
			{
				Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(message.x, message.y, message.z), false);
			}

			Client.RequestChunkRadius(Client.ChunkRadius);

			_changeDimensionResetEvent.Set();
		}

		public void HandleMcpeContainerOpen(McpeContainerOpen message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeContainerClose(McpeContainerClose message)
		{
			UnhandledPackage(message);
		}

        public void HandleMcpeInventoryContent(McpeInventoryContent message)
		{
			Inventory inventory = null;
			if (message.inventoryId == 0x00)
			{
				inventory = Client.WorldReceiver.Player.Inventory;
			}

			if (inventory == null || message.input == null) return;

			for (var index = 0; index < message.input.Count; index++)
			{
				var slot = message.input[index];
				
				var usedIndex = index;

				var result = ToAlexItem(slot);
				if (result != null)
				{
					inventory[usedIndex] = result;
				}
				else
                {
                    Log.Warn($"Failed to set slot: {index} Id: {slot.Id}:{slot.Metadata}");
                }
            }
		}

        private Item ToAlexItem(MiNET.Items.Item item)
        {
	        Item result = null;
			
	        if (item.Id < 256) //Block
	        {
		        var id = item.Id;
		        var meta = (byte)item.Metadata;
		        var reverseMap = MiNET.Worlds.AnvilWorldProvider.Convert.FirstOrDefault(map =>
			        map.Value.Item1 == id);

		        if (reverseMap.Value != null)
		        {
			        id = (byte) reverseMap.Key;
		        }
									        
		        var res = BlockFactory.GetBlockStateID(id, meta);

		        if (AnvilWorldProvider.BlockStateMapper.TryGetValue(res,
			        out var res2))
		        {
			        var t = BlockFactory.GetBlockState(res2);

			        ItemFactory.TryGetItem(t.Name, out result);
		        }
		        else
		        {
			        var block = BlockFactory.RuntimeIdTable.FirstOrDefault(x => x.Id == item.Id);
			        if (block != null)
			        {
				        ItemFactory.TryGetItem(block.Name, out result);
			        }
		        }

		        if (result != null)
		        {
			        result.Id = item.Id;
			        //result.Meta = item.Metadata;
		        }
	        }
			
	        if (result == null)
	        {
		        ItemFactory.TryGetItem(item.Id, item.Metadata, out result);
		        //  Log.Info($"Set inventory slot: {message.slot} Id: {message.item.Id}:{message.item.Metadata} x {message.item.Count} Name: {item.DisplayName} IsPeInv: {inventory.IsPeInventory}");
	        }

	        if (result != null)
	        {
		        result.Meta = item.Metadata;
		        result.Count = item.Count;
		        result.Nbt = item.ExtraData;
				
		        return result;
	        }

	        return null;
        }
        
		public void HandleMcpeInventorySlot(McpeInventorySlot message)
		{
			Inventory inventory = null;

			if (message.inventoryId == 0x00)
			{
				inventory = Client.WorldReceiver.Player.Inventory;
			}

			if (inventory == null || message.item == null) return;
			
			var index = (int)message.slot;
			var result = ToAlexItem(message.item);

			if (result != null)
            {
	            inventory[index] = result;
            }
            else
            {
                Log.Warn($"Failed to set slot: {message.slot} Id: {message.item.Id}:{message.item.Metadata}");
            }
		}

        public void HandleMcpePlayerHotbar(McpePlayerHotbar message)
        {
            UnhandledPackage(message);
        }

        public void HandleMcpeContainerSetData(McpeContainerSetData message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeCraftingData(McpeCraftingData message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeCraftingEvent(McpeCraftingEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeGuiDataPickItem(McpeGuiDataPickItem message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeBlockEntityData(McpeBlockEntityData message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeLevelChunk(McpeLevelChunk msg)
		{
			var cacheEnabled = msg.cacheEnabled;
			var subChunkCount = msg.subChunkCount;
			var chunkData = msg.chunkData;
			var cx = msg.chunkX;
			var cz = msg.chunkZ;
			msg.PutPool();

			//if (chunkData[0] < 1)
			if (subChunkCount < 1)
			{
				//Nothing to read.
				return;
			}
			
			ChunkProcessor.HandleChunkData(cacheEnabled, subChunkCount, chunkData, cx, cz,
				column =>
				{ 
					Client.WorldReceiver.ChunkManager.AddChunk(column, new ChunkCoordinates(column.X, column.Z), true);
					
					EventDispatcher.DispatchEvent(
						new ChunkReceivedEvent(new ChunkCoordinates(column.X, column.Z), column));
				});
		}

		private AutoResetEvent _changeDimensionResetEvent = new AutoResetEvent(false);
		public void HandleMcpeChangeDimension(McpeChangeDimension message)
		{
			//base.HandleMcpeChangeDimension(message);
			if (WorldProvider is BedrockWorldProvider provider)
			{
				var chunkCoords =
					new ChunkCoordinates(new PlayerLocation(message.position.X, message.position.Y,
						message.position.Z));
				
				LoadingWorldState loadingWorldState = new LoadingWorldState();

				AlexInstance.GameStateManager.SetActiveState(loadingWorldState, true);
				loadingWorldState.UpdateProgress(LoadingState.LoadingChunks, 0);

				ThreadPool.QueueUserWorkItem((o) =>
				{
					McpePlayerAction action = McpePlayerAction.CreateObject();
					action.runtimeEntityId = Client.EntityId;
					action.actionId = (int) PlayerAction.DimensionChangeAck;
					Client.SendPacket(action);

					World world = Client.WorldReceiver;

					world.ChunkManager.ClearChunks();
					world.UpdatePlayerPosition(new PlayerLocation(message.position.X, message.position.Y, message.position.Z));
					
				
					foreach (var loadedChunk in provider.LoadedChunks)
					{
						provider.UnloadChunk(loadedChunk);
					}
					
					int percentage = 0;
					bool ready = false;
					int previousPercentage = 0;
					bool spawnChunkLoaded = false;
					
					do
					{
						if (!spawnChunkLoaded && percentage >= 100)
						{
							loadingWorldState.UpdateProgress(LoadingState.Spawning, 99);
						}
						else
						{
							double radiusSquared = Math.Pow(Client.ChunkRadius, 2);
							var target = radiusSquared;

							percentage = (int) ((100 / target) * world.ChunkManager.ChunkCount);

							if (percentage != previousPercentage)
							{
								loadingWorldState.UpdateProgress(LoadingState.LoadingChunks,
									percentage);
								previousPercentage = percentage;

								//Log.Info($"Progress: {percentage} ({ChunksReceived} of {target})");
							}
						}

						//if (!ready)
						//{
						if (!ready)
						{
							if (_changeDimensionResetEvent.WaitOne(5))
							{
								ready = true;
							}
						}

						if (!spawnChunkLoaded && ready)
						{
							if (provider.LoadedChunks.Contains(chunkCoords))
							{
								spawnChunkLoaded = true;
							}
						}


						if (percentage >= 100 && ready && spawnChunkLoaded)
						{
							break;
						}

						//	}
						//	else
						//	{
						//	await Task.Delay(50);
						//}
					} while (true);

					AlexInstance.GameStateManager.Back();

					
					Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(message.position.X, message.position.Y, message.position.Z), false);
				});
			}
		}
		
		public void HandleMcpeSetCommandsEnabled(McpeSetCommandsEnabled message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetDifficulty(McpeSetDifficulty message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetPlayerGameType(McpeSetPlayerGameType message)
		{
			Client.WorldReceiver.Player.UpdateGamemode((Gamemode) message.gamemode);
		}

		public void HandleMcpeSimpleEvent(McpeSimpleEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeTelemetryEvent(McpeTelemetryEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSpawnExperienceOrb(McpeSpawnExperienceOrb message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeClientboundMapItemData(McpeClientboundMapItemData message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMapInfoRequest(McpeMapInfoRequest message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeRequestChunkRadius(McpeRequestChunkRadius message)
		{
			 Client.RequestChunkRadius(Client.ChunkRadius);
		}

		public void HandleMcpeChunkRadiusUpdate(McpeChunkRadiusUpdate message)
		{
			Client.ChunkRadius = message.chunkRadius;
			//UnhandledPackage(message);
		}

		public void HandleMcpeItemFrameDropItem(McpeItemFrameDropItem message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeGameRulesChanged(McpeGameRulesChanged message)
		{
			var lvl = Client.WorldReceiver.Player.Level;
			
			foreach (var gr in message.rules)
			{
				lvl.SetGameRule(gr);
			}
		}

		public void HandleMcpeCamera(McpeCamera message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeBossEvent(McpeBossEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeShowCredits(McpeShowCredits message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeAvailableCommands(McpeAvailableCommands message)
		{
			// Client.LoadCommands(message.CommandSet);
			UnhandledPackage(message);
		}

		public void HandleMcpeCommandOutput(McpeCommandOutput message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateTrade(McpeUpdateTrade message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateEquipment(McpeUpdateEquipment message)
		{
			UnhandledPackage(message);
		}
		
		public void HandleMcpeTransfer(McpeTransfer message)
		{
			 Client.SendDisconnectionNotification();
			 WorldProvider.Dispose();
			 
			 ThreadPool.QueueUserWorkItem(o =>
			 {

				 IPHostEntry hostEntry = Dns.GetHostEntry(message.serverAddress);

				 if (hostEntry.AddressList.Length > 0)
				 {
					 var ip = hostEntry.AddressList[0];
					 AlexInstance.ConnectToServer(new IPEndPoint(ip, message.port), PlayerProfile, true);
				 }
			 });
		}

		public void HandleMcpePlaySound(McpePlaySound message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeStopSound(McpeStopSound message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetTitle(McpeSetTitle message)
		{
			var titleComponent = WorldProvider?.TitleComponent;
			if (titleComponent == null)
				return;
			
			switch ((TitleType) message.type)
			{
				case TitleType.Clear:
					titleComponent.Hide();
					break;
				case TitleType.Reset:
					titleComponent.Reset();
					break;
				case TitleType.Title:
					titleComponent.SetTimes(message.fadeInTime, message.stayTime, message.fadeOutTime);
					titleComponent.SetTitle(new ChatObject(message.text));
					titleComponent.Show();
					break;
				case TitleType.SubTitle:
					titleComponent.SetTimes(message.fadeInTime, message.stayTime, message.fadeOutTime);
					titleComponent.SetSubtitle(new ChatObject(message.text));
					titleComponent.Show();
					break;
				case TitleType.ActionBar:
					
					break;
				case TitleType.AnimationTimes:
					titleComponent.SetTimes(message.fadeInTime, message.stayTime, message.fadeOutTime);
					break;
			}
	
			//UnhandledPackage(message);
		}

		public void HandleMcpeAddBehaviorTree(McpeAddBehaviorTree message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeStructureBlockUpdate(McpeStructureBlockUpdate message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeShowStoreOffer(McpeShowStoreOffer message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpePlayerSkin(McpePlayerSkin message)
		{
			//TODO: Load skin
			UnhandledPackage(message);
		}

		public void HandleMcpeSubClientLogin(McpeSubClientLogin message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeInitiateWebSocketConnection(McpeInitiateWebSocketConnection message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetLastHurtBy(McpeSetLastHurtBy message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeBookEdit(McpeBookEdit message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeNpcRequest(McpeNpcRequest message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeModalFormRequest(McpeModalFormRequest message)
		{
			Form form = JsonConvert.DeserializeObject<Form>(
				message.data, new FormConverter(), new CustomElementConverter());

			Client.WorldReceiver.FormManager.Show(message.formId, form);
			//UnhandledPackage(message);
		}

		public void HandleMcpeServerSettingsResponse(McpeServerSettingsResponse message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeShowProfile(McpeShowProfile message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetDefaultGameType(McpeSetDefaultGameType message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeRemoveObjective(McpeRemoveObjective message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetDisplayObjective(McpeSetDisplayObjective message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetScore(McpeSetScore message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeLabTable(McpeLabTable message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateBlockSynced(McpeUpdateBlockSynced message)
		{
			UnhandledPackage(message);
		}

        public void HandleMcpeSetScoreboardIdentityPacket(McpeSetScoreboardIdentityPacket message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateSoftEnumPacket(McpeUpdateSoftEnumPacket message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeNetworkStackLatencyPacket(McpeNetworkStackLatencyPacket message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeScriptCustomEventPacket(McpeScriptCustomEventPacket message)
		{
			UnhandledPackage(message);
		}

		public void HandleFtlCreatePlayer(FtlCreatePlayer message)
		{
			UnhandledPackage(message);
		}
	}
}
