using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using Alex.API.Events;
using Alex.API.Events.World;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Entities;
using Alex.Entities.Generic;
using Alex.Entities.Projectiles;
using Alex.Gamestates;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Animations;
using Alex.Items;
using Alex.Net.Bedrock;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Utils.Inventories;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Singleplayer;
using Jose;
using Microsoft.Extensions.DependencyInjection;
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
using SixLabors.ImageSharp.PixelFormats;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using MathF = System.MathF;
using MessageType = Alex.API.Data.MessageType;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Worlds.Multiplayer.Bedrock
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
	        //Plugin = plugin;
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
		       // var data = JWT.Decode<HandshakeData>(token, signKey);
				var data = JWT.Payload<HandshakeData>(token);
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

				Client.World.Player.EntityId = Client.EntityId;
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
			EventDispatcher.DispatchEvent(new ChatMessageReceivedEvent(new ChatObject(message.message), (MessageType) message.type));
		}

		public void HandleMcpeSetTime(McpeSetTime message)
		{
			Client.World?.SetTime(message.time);
			
			Client.ChangeDimensionResetEvent.Set();
		}

		public void HandleMcpeStartGame(McpeStartGame message)
		{
			Client.GameStarted = true;
			
			Client.EntityId = message.runtimeEntityId;
			Client.NetworkEntityId = message.entityIdSelf;
			Client.SpawnPoint = new Vector3(message.spawn.X, message.spawn.Y - Player.EyeLevel, message.spawn.Z); //message.spawn;
			//Client.CurrentLocation = new MiNET.Utils.PlayerLocation(Client.SpawnPoint, message.spawn.X, message.spawn.X, message.spawn.Y);

			Client.World?.UpdatePlayerPosition(
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
			
			Client.World.Player.EntityId = message.runtimeEntityId;
			Client.World.Player.UpdateGamemode((Gamemode) message.playerGamemode);

			foreach (var gr in message.gamerules)
			{
				Client.World.SetGameRule(gr);
			}

			//Client.World.Player.Inventory.IsPeInventory = true;

			_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
		}

		public void HandleMcpeMovePlayer(McpeMovePlayer message)
		{
			if (message.runtimeEntityId != Client.EntityId)
			{
				Client.World.UpdateEntityPosition(message.runtimeEntityId, 
					new PlayerLocation(message.x, message.y - Player.EyeLevel, message.z, message.headYaw, message.yaw, -message.pitch));
				return;
			}
			
			Client.World.UpdatePlayerPosition(new 
				PlayerLocation(message.x, message.y, message.z));

			// Client.SendMcpeMovePlayer();
		//	Client.CurrentLocation = new MiNET.Utils.PlayerLocation(message.x, message.y, message.z);
			Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(message.x, message.y, message.z), false);
			
			//_changeDimensionResetEvent.Set();
		}


		public void HandleMcpeAdventureSettings(McpeAdventureSettings message)
		{
			//Client.UserPermission = (CommandPermission) message.commandPermission;
			Client.World.Player.CanFly = ((message.flags & 0x40) == 0x40);
			Client.World.Player.IsFlying = ((message.flags & 0x200) == 0x200);
			Client.World.Player.IsWorldImmutable = ((message.flags & 0x01) == 0x01);
			Client.World.Player.IsNoPvP = (message.flags & 0x02) == 0x02;
			Client.World.Player.IsNoPvM = (message.flags & 0x04) == 0x04;
		}

		private ConcurrentDictionary<MiNET.Utils.UUID, RemotePlayer> _players = new ConcurrentDictionary<MiNET.Utils.UUID, RemotePlayer>();
        public void HandleMcpeAddPlayer(McpeAddPlayer message)
		{
		//	UUID u = new UUID(message.uuid.GetBytes());
			if (_players.TryGetValue(message.uuid, out RemotePlayer mob))
			{
				//MiNET.Player
				mob.EntityId = message.runtimeEntityId;
				mob.KnownPosition = new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch);
				mob.Velocity = new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ);

				if (string.IsNullOrWhiteSpace(mob.NameTag) && !string.IsNullOrWhiteSpace(message.username))
				{
					//mob.NameTag = message.username;
					//mob.HideNameTag = false;
				}

				if (message.item != null)
				{
					mob.Inventory.MainHand = ToAlexItem(message.item);
				}

				if (message.metadata != null)
				{
					mob.HandleMetadata(message.metadata);
				}
				//mob.Height = message.metadata[(int) MiNET.Entities.Entity.MetadataFlags.CollisionBoxHeight]
				
				//if (Client.World is World w)
				{
					mob.IsSpawned = true;
					
					_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
					Client.World.SpawnEntity(mob.EntityId, mob);
				}
				//else
				{
				//	mob.IsSpawned = false;
				}
			}
			else
			{
				Log.Warn($"Tried spawning invalid player: {message.uuid}");
			}
		}

        private static JsonSerializerSettings GeometrySerializationSettings = new JsonSerializerSettings()
        {
	        Converters = new List<JsonConverter>()
	        {
		        new SingleOrArrayConverter<Vector3>(),
		        new SingleOrArrayConverter<Vector2>(),
		        new Vector3Converter(),
		        new Vector2Converter()
	        },
	        MissingMemberHandling = MissingMemberHandling.Ignore
        };
        
        private ConcurrentDictionary<string, EntityModel> _models = new ConcurrentDictionary<string, EntityModel>();
        private bool ProcessEntityModel(EntityModel model, bool isRetry = false)
        {
	        string modelName = model.Name;
            if (model.Name.Contains(":")) //This model inherits from another model.
            {
                string[] split = model.Name.Split(':');
                string parent = split[1];

                EntityModel parentModel;
                if (!ModelFactory.TryGetModel(parent, out parentModel) && !_models.TryGetValue(parent, out parentModel))
                {
                    if (!isRetry)
                    {
                       // failedToProcess.TryAdd(modelName, model);

                        Log.Warn($"No parent model for {modelName}");
                    }

                    return false;
                }

                modelName = split[0];

                if (model.Bones == null)
                {
                    model.Bones = new EntityModelBone[0];
                }

                if (parentModel == null)
                {
                    Log.Warn($"Pass 1 fail... {modelName}");
                    return false;
                }

                if (parentModel.Bones == null || parentModel.Bones.Length == 0)
                {
                    Log.Warn($"Parent models contains no bones! {modelName}");
                    return false;
                }

                Dictionary<string, EntityModelBone> parentBones =
                    parentModel.Bones.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name))
                        .ToDictionary(x => x.Name, e => e);

                Dictionary<string, EntityModelBone> bones =
                    model.Bones.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name))
                        .ToDictionary(x => x.Name, e => e);

                foreach (var bone in parentBones)
                {
                    var parentBone = bone.Value;
                    if (bones.TryGetValue(bone.Key, out EntityModelBone val))
                    {
                        if (!val.Reset)
                        {
                            if (val.Cubes != null)
                            {
                                val.Cubes = val.Cubes.Concat(parentBone.Cubes).ToArray();
                            }
                            else
                            {
                                val.Cubes = parentBone.Cubes;
                            }

                            //val.Cubes.Concat(parentBone.Cubes);
                        }


                        bones[bone.Key] = val;
                    }
                    else
                    {
                        bones.Add(bone.Key, parentBone);
                    }
                }

                model.Bones = bones.Values.ToArray();
            }

            if (model != null)
            {
	            foreach (var bone in model.Bones)
	            {
		            if (bone != null && bone.Cubes != null)
		            {
			            foreach (var cube in bone.Cubes)
			            {
				            cube.Origin = new Microsoft.Xna.Framework.Vector3(
					            -(cube.Origin.X + cube.Size.X), cube.Origin.Y, cube.Origin.Z);
			            }
		            }
	            }
            }

            return _models.TryAdd(modelName, model);
        }
        
		public void HandleMcpePlayerList(McpePlayerList message)
		{
			if (message.records is PlayerAddRecords addRecords)
			{
				foreach (var r in addRecords)
				{
					if (_players.ContainsKey(r.ClientUuid)) continue;
					
					var u = new API.Utils.UUID(r.ClientUuid.GetBytes());
					
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

					Client.World?.AddPlayerListItem(new PlayerListItem(u, r.DisplayName, Gamemode.Survival, 0));

					EntityModelRenderer renderer = null;
					if (UseCustomEntityModels)
					{
						if (!string.IsNullOrWhiteSpace(r.Skin.GeometryData) && r.Skin.GeometryData != "null")
						{
							try
							{
								EntityModel model;
								var resourcePatch = JsonConvert.DeserializeObject<SkinResourcePatch>(r.Skin.ResourcePatch, GeometrySerializationSettings);

								if (!_models.TryGetValue(resourcePatch.Geometry.Default, out model))
								{
									Dictionary<string, EntityModel> models = new Dictionary<string, EntityModel>();
									EntityModel.GetEntries(r.Skin.GeometryData, models);

									foreach (var mm in models.ToArray())
									{
										if (ProcessEntityModel(mm.Value, false))
										{
											models.Remove(mm.Key);
										}
									}

									foreach (var mm in models.ToArray())
									{
										if (ProcessEntityModel(mm.Value, true))
										{
											models.Remove(mm.Key);
										}
									}

									_models.TryGetValue(resourcePatch.Geometry.Default, out model);
								}

								if (model != null)
								{
									var modelRenderer = new EntityModelRenderer(model, skinTexture);

									if (modelRenderer.Valid)
									{
										renderer = modelRenderer;
									}
									else
									{
										modelRenderer.Dispose();
									}
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

					RemotePlayer m = new RemotePlayer(r.DisplayName, Client.World as World, Client, skinTexture);
					m.UUID = u;
					m.EntityId = r.EntityId;
					m.SetInventory(new BedrockInventory(46));
					
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
			            Client.World?.RemovePlayerListItem(player.UUID);
			            if (Client.World is World w)
			            {
				            //w.DespawnEntity(player.EntityId);
			            }
		            }
	            }
            }
		}

		public bool SpawnMob(long entityId,
			Guid uuid,
			EntityType type,
			PlayerLocation position,
			Microsoft.Xna.Framework.Vector3 velocity, EntityAttributes attributes)
		{
			Entity entity = null;

			if (type == EntityType.FallingBlock)
			{
				entity = new EntityFallingBlock(null);
			}
			else
			{
				EntityModelRenderer renderer = EntityFactory.GetEntityRenderer(
					$"minecraft:{type.ToString().ToLower()}", null);

				if (renderer != null)
				{
					entity = type.Create(null);
				}

				if (entity == null)
				{
					entity = new Entity((int) type, null, Client);
				}

				if (renderer == null)
				{
					Log.Debug($"Missing renderer for entity: {type.ToString()} ({(int) type})");

					return false;
				}

				if (renderer.Texture == null)
				{
					Log.Debug($"Missing texture for entity: {type.ToString()} ({(int) type})");

					return false;
				}

				entity.ModelRenderer = renderer;
			}

			entity.KnownPosition = position;
			entity.Velocity = velocity;
			entity.EntityId = entityId;
			entity.UUID = new UUID(uuid.ToByteArray());
			entity.SetInventory(new BedrockInventory(46));
			
			Client.World.SpawnEntity(entityId, entity);

			return true;
		}


		public void HandleMcpeAddEntity(McpeAddEntity message)
		{
			var type = message.entityType.Replace("minecraft:", "").Replace("_", "");
			if (Enum.TryParse(typeof(EntityType), type, true, out object res))
			{
				SpawnMob(message.runtimeEntityId, Guid.NewGuid(), (EntityType) res,
					new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch),
					new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ),
					message.attributes);
				_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
			}
			else
            {
	           /* if (SpawnMob(message.runtimeEntityId, Guid.NewGuid(), ,
		            new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch),
		            new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ) * 20f,
		            message.attributes))
	            {
		            
	            }*/
	            Log.Warn($"Unknown mob: {type}");
            }
        }

		private ConcurrentDictionary<long, long> _entityMapping = new ConcurrentDictionary<long, long>();
		public void HandleMcpeRemoveEntity(McpeRemoveEntity message)
		{
			if (_entityMapping.TryRemove(message.entityIdSelf, out var entityId))
			{
				Client.World.DespawnEntity(entityId);
			}
		}

		public void HandleMcpeAddItemEntity(McpeAddItemEntity message)
		{
			var slot = message.item;
			var item = ToAlexItem(message.item);
			//if (ItemFactory.TryGetItem(slot.Id, slot.Metadata, out Item item))
			{
				var itemClone = item.Clone();
				
				itemClone.Count = slot.Count;
				itemClone.Nbt = slot.ExtraData;

				ItemEntity itemEntity = new ItemEntity(null);
				itemEntity.EntityId = message.runtimeEntityId;
				itemEntity.Velocity = new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ);
				itemEntity.KnownPosition = new PlayerLocation(message.x, message.y, message.z);
				itemEntity.HandleMetadata(message.metadata);
				
				itemEntity.SetItem(itemClone);

				if (Client.World.SpawnEntity(message.runtimeEntityId, itemEntity))
				{
					_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
				}
				else
				{
					Log.Warn($"Could not spawn in item entity, an entity with this runtimeEntityId already exists! (Runtime: {message.runtimeEntityId} Self: {message.entityIdSelf})");
				}

				// Log.Info($"Set inventory slot: {usedIndex} Id: {slot.Id}:{slot.Metadata} x {slot.Count} Name: {item.DisplayName} IsPeInv: {inventory.IsPeInventory}");
			}
		}

		public void HandleMcpeTakeItemEntity(McpeTakeItemEntity message)
		{
			if (Client.World.EntityManager.TryGet(message.target, out var targetEntity)
			    && Client.World.EntityManager.TryGet(message.runtimeEntityId, out var runtimeEntity))
			{
				ItemEntity itemEntity;
				Entity target;

				if (runtimeEntity is ItemEntity rte)
				{
					itemEntity = rte;
					target = targetEntity;
				}
				else if (targetEntity is ItemEntity rte2)
				{
					itemEntity = rte2;
					target = runtimeEntity;
				}
				else
				{
					Log.Warn($"Invalid TakeItemEntity request.");
					return;
				}

				var targetBoundingBox = target.GetBoundingBox(target.KnownPosition);

				var direction = (itemEntity.KnownPosition.ToVector3() - new Microsoft.Xna.Framework.Vector3(
					target.KnownPosition.X, targetBoundingBox.Max.Y, target.KnownPosition.Z));

				//direction.Normalize();

				itemEntity.Velocity = direction / 20f;
			}
		}

		public void HandleMcpeMoveEntity(McpeMoveEntity message)
		{
			var location = new PlayerLocation(
				message.position.X, message.position.Y, message.position.Z, message.position.HeadYaw,
				message.position.Yaw, message.position.Pitch);

			if (message.runtimeEntityId != Client.EntityId)
			{
				Client.World.UpdateEntityPosition(message.runtimeEntityId, location);

				return;
			}

			Client.World.UpdatePlayerPosition(location);
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

					if (Client.World.TryGetEntity(message.runtimeEntityId, out var entity))
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
				Client.World?.SetBlockState(
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

		public void HandleMcpeNetworkSettings(McpeNetworkSettings message)
		{
			UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpeCreativeContent(McpeCreativeContent message)
		{
			UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpePlayerEnchantOptions(McpePlayerEnchantOptions message)
		{
			UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpeItemStackResponse(McpeItemStackResponse message)
		{
			UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpeAlexEntityAnimation(McpeAlexEntityAnimation message)
		{
			Entity entity;
			if (!Client.World.EntityManager.TryGet(message.runtimeEntityId, out entity))
			{
				if (message.runtimeEntityId != Client.World.Player.EntityId)
				{
					Log.Warn($"Got animation request for unknown entity: {entity}");

					return;
				}
				
				entity = Client.World.Player;
			}

			if (!entity.ModelRenderer.GetBone(message.boneId, out var bone))
			{
				Log.Warn($"Animation issue: Could not find bone with name {message.boneId}");

				return;
			}

			if (message.keys.Length == 0)
			{
				Log.Warn($"Invalid animation: 0 keys.");

				return;
			}

			if (message.keys.First().ExecuteImmediate)
			{
				bone.ClearAnimations();
			}

			foreach (var key in message.keys)
			{
				bone.Animations.Enqueue(
					new ServerAnimation(
						bone,
						new ModelParameters(
							new Microsoft.Xna.Framework.Vector3(
								key.StartRotation.X, key.StartRotation.Y, key.StartRotation.Z),
							Microsoft.Xna.Framework.Vector3.Zero),
						new ModelParameters(
							new Microsoft.Xna.Framework.Vector3(
								key.EndRotation.X, key.EndRotation.Y, key.EndRotation.Z),
							Microsoft.Xna.Framework.Vector3.Zero), TimeSpan.FromMilliseconds(key.Duration), key.ResetAfter));
			}

			bone.Animations.Enqueue(new ResetAnimation(bone));
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
			if (!Client.World.Player.IsBreakingBlock)
				return;

			if ((BlockCoordinates) new Microsoft.Xna.Framework.Vector3(
				message.position.X, message.position.Y, message.position.Z) == Client.World.Player.TargetBlock)
			{
				if (message.eventId == 3600)
				{
					var ticksRequired = (double) ushort.MaxValue / message.data;
					Client.World.Player.BreakTimeNeeded = ticksRequired;
				}
			}
		}

		public void HandleMcpeBlockEvent(McpeBlockEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeEntityEvent(McpeEntityEvent message)
		{
			if (Client.World.TryGetEntity(message.runtimeEntityId, out Entity entity))
			{
				if (message.eventId == 2)
				{
					entity.EntityHurt();
				}
				else if (message.eventId == 4)
				{
					entity.SwingArm();
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
			var player = Client.World.Player;
			if (player.EntityId == message.runtimeEntityId)
			{
				if (message.attributes.TryGetValue("minecraft:health", out var value))
				{
					player.HealthManager.MaxHealth = (int) value.MaxValue;
					player.HealthManager.Health = (int) value.Value;
				}

				if (message.attributes.TryGetValue("minecraft:movement", out var movement))
				{
					player.MovementSpeed = movement.Value;
				}

				if (message.attributes.TryGetValue("minecraft:player.hunger", out var hunger))
				{
					player.HealthManager.Hunger = (int) hunger.Value;
					player.HealthManager.MaxHunger = (int) hunger.MaxValue;
				}
				
				if (message.attributes.TryGetValue("minecraft:player.exhaustion", out var exhaustion))
				{
					player.HealthManager.Exhaustion = (int) exhaustion.Value;
					player.HealthManager.MaxExhaustion = (int) exhaustion.MaxValue;
				}
				
				if (message.attributes.TryGetValue("minecraft:player.saturation", out var saturation))
				{
					player.HealthManager.Saturation = (int) saturation.Value;
					player.HealthManager.MaxSaturation = (int) saturation.MaxValue;
				}
			}
		}

		public void HandleMcpeInventoryTransaction(McpeInventoryTransaction message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMobEquipment(McpeMobEquipment message)
		{
			if (Client.World.Player.EntityId == message.runtimeEntityId)
			{
				Client.World.Player.Inventory.SelectedSlot = message.selectedSlot;
				return;
			}
			
			if (Client.World.TryGetEntity(message.runtimeEntityId, out var entity))
			{
				var item = ToAlexItem(message.item).Clone();

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
			Entity entity = null;

			if (!Client.World.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (Client.World.Player.EntityId == message.runtimeEntityId)
				{
					entity = Client.World.Player;
				}

				//entity.Inventory.Boots
			}

			if (entity == null)
			{
				Log.Warn($"Unknown entity in MobArmorEquipment packet!");

				return;
			}


			entity.Inventory.Helmet = ToAlexItem(message.helmet).Clone();
			entity.Inventory.Chestplate = ToAlexItem(message.chestplate).Clone();
			entity.Inventory.Leggings = ToAlexItem(message.leggings).Clone();
			entity.Inventory.Boots = ToAlexItem(message.boots).Clone();


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
			if (!Client.World.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (Client.World.Player.EntityId == message.runtimeEntityId)
				{
					entity = Client.World.Player;
				}
			}

			if (entity == null)
				return;

			if (entity is EntityFallingBlock fallingBlock)
			{
				foreach (var meta in message.metadata._entries.ToArray())
				{
					switch ((MiNET.Entities.Entity.MetadataFlags) meta.Key)
					{
						case MiNET.Entities.Entity.MetadataFlags.Variant:
							message.metadata._entries.Remove(meta.Key);

							if (meta.Value is MetadataInt metaInt)
							{
								var blockState = ChunkProcessor.GetBlockState((uint) metaInt.Value);

								if (ItemFactory.TryGetItem(blockState.Name, out var item))
								{
									fallingBlock.SetItem(item);
								}
								else
								{
									Log.Info($"Could not get item: {blockState.Name}");
								}
							}

							break;
					}
				}
			}

			entity.HandleMetadata(message.metadata);
			//UnhandledPackage(message);
		}

		public void HandleMcpeSetEntityMotion(McpeSetEntityMotion message)
		{
			var v = message.velocity;
			var velocity = new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z);

			Entity entity = null;
			if (!Client.World.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (Client.World.Player.EntityId == message.runtimeEntityId)
				{
					entity = Client.World.Player;
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
			Client.World.Player.HealthManager.Health = message.health;
		}

		public void HandleMcpeSetSpawnPosition(McpeSetSpawnPosition message)
		{
			Client.SpawnPoint = new Vector3(
				message.coordinates.X, (float) (message.coordinates.Y), message.coordinates.Z);

			Client.LevelInfo.SpawnX = (int) Client.SpawnPoint.X;
			Client.LevelInfo.SpawnY = (int) Client.SpawnPoint.Y;
			Client.LevelInfo.SpawnZ = (int) Client.SpawnPoint.Z;

			Client.World.SpawnPoint = new Microsoft.Xna.Framework.Vector3(
				Client.SpawnPoint.X, Client.SpawnPoint.Y, Client.SpawnPoint.Z);
		}

		public void HandleMcpeAnimate(McpeAnimate message)
		{
			if (Client.World.EntityManager.TryGet(message.runtimeEntityId, out Entity entity))
			{
				switch (message.actionId)
				{
					case 1:
						entity.SwingArm();
						break;
					//case 4: //Critical hit!
						//entity.EntityHurt();
					//	break;
					default:
						UnhandledPackage(message);

						break;
				}
			}
		}

		public void HandleMcpeRespawn(McpeRespawn message)
		{
			Client.World.UpdatePlayerPosition(new PlayerLocation(message.x, message.y, message.z));

			if (Client.PlayerStatus == 3)
			{
				Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(message.x, message.y, message.z), false);
			}

			Client.RequestChunkRadius(Client.ChunkRadius);

			Client.ChangeDimensionResetEvent.Set();
		}

		private GuiDialogBase _activeDialog;
		public void HandleMcpeContainerOpen(McpeContainerOpen message)
		{
			try
			{
				var windowId = message.windowId;
				var dialog = Client.World.InventoryManager.Show(Client.World.Player.Inventory, message.windowId, (ContainerType) message.type);
				dialog.OnContainerClose += (sender, args) =>
				{
					var packet = McpeContainerClose.CreateObject();
					packet.windowId = windowId;
					Client.SendPacket(packet);
				};
			}
			catch (Exception)
			{
				Log.Warn($"Got unsupported container type: {message.type}");
			}
		}

		public void HandleMcpeContainerClose(McpeContainerClose message)
		{
			if (_activeDialog != null)
			{
				AlexInstance.GuiManager.HideDialog(_activeDialog);
			}
		}

        public void HandleMcpeInventoryContent(McpeInventoryContent message)
		{
			if (message.input == null)
			{
				Log.Warn($"Invalid inventory content! Input was null.");
				return;
			}
			
			InventoryBase inventory = null;
			if (message.inventoryId == 0x00)
			{
				inventory = Client.World.Player.Inventory;
			}

			if (inventory == null)
			{
				Log.Warn($"Unknown inventory ID: {message.inventoryId}");
				return;
			}

			for (var index = 0; index < message.input.Count; index++)
			{
				var slot = message.input[index];
				
				var usedIndex = index;

				var result = ToAlexItem(slot).Clone();
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
	        if (item == null)
		        return new ItemAir();
	        
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
		        result.StackID = item.UniqueId;
		        result.Meta = item.Metadata;
		        result.Count = item.Count;
		        result.Nbt = item.ExtraData;
				
		        return result;
	        }

	        return new ItemAir()
	        {
		        Count = 0
	        };
        }
        
		public void HandleMcpeInventorySlot(McpeInventorySlot message)
		{
			if (message.item == null)
			{
				Log.Warn($"Invalid inventory slot packet! Item was null.");
				return;
			}
			
			InventoryBase inventory = null;

			if (message.inventoryId == 0x00)
			{
				inventory = Client.World.Player.Inventory;
			}

			if (inventory == null)
			{
				Log.Warn($"Unknown inventory ID: {message.inventoryId}");
				return;
			}
			
			var index = (int)message.slot;
			var result = ToAlexItem(message.item).Clone();

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
					Client.World.ChunkManager.AddChunk(column, new ChunkCoordinates(column.X, column.Z), true);
					
					//EventDispatcher.DispatchEvent(
					//	new ChunkReceivedEvent(new ChunkCoordinates(column.X, column.Z), column));
				});
		}
		
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

					World world = Client.World;

					world.ClearChunksAndEntities();
					//world.ChunkManager.ClearChunks();
					world.UpdatePlayerPosition(new PlayerLocation(message.position.X, message.position.Y, message.position.Z));
					
				
					//foreach (var loadedChunk in provider.LoadedChunks)
					//{
					//	provider.UnloadChunk(loadedChunk);
					//}
					
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
							if (Client.ChangeDimensionResetEvent.WaitOne(5))
							{
								ready = true;
							}
						}

						if (!spawnChunkLoaded && ready)
						{
							if (world.ChunkManager.TryGetChunk(chunkCoords, out _))
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
			Client.World.Player.UpdateGamemode((Gamemode) message.gamemode);
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
			var lvl = Client.World.Player.Level;
			
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
					 AlexInstance.ConnectToServer(new BedrockServerType(AlexInstance, AlexInstance.Services.GetService<XboxAuthService>()), new ServerConnectionDetails(new IPEndPoint(ip, message.port)), PlayerProfile);
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

			Client.WorldProvider.FormManager.Show(message.formId, form);
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
			var scoreboard = WorldProvider?.ScoreboardView;
			if (scoreboard == null)
				return;
			
			switch (message.displaySlot)
			{
				case "sidebar":
					scoreboard.Clear();
					scoreboard.AddString(message.displayName);
					break;
			}
		}

		public void HandleMcpeSetScore(McpeSetScore message)
		{
			var scoreboard = WorldProvider?.ScoreboardView;
			if (scoreboard == null)
				return;
			
			foreach (var entry in message.entries)
			{
				if (entry is ScoreEntryChangeFakePlayer fakePlayer)
				{
					scoreboard.AddRow($"{fakePlayer.ObjectiveName}:{fakePlayer.Id}", fakePlayer.CustomName, entry.Score);
				}
				else if (entry is ScoreEntryChangePlayer player)
				{
					if (Client.World.EntityManager.TryGet(player.EntityId, out var playerEntity))
					{
						scoreboard.AddRow($"{player.ObjectiveName}:{player.Id}", playerEntity.NameTag, player.Score);
					}
				}
				else if (entry is ScoreEntryRemove remove)
				{
					scoreboard.Remove($"{remove.ObjectiveName}:{remove.Id}");
				}
			}
		}

		public void HandleMcpeLabTable(McpeLabTable message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateBlockSynced(McpeUpdateBlockSynced message)
		{
			UnhandledPackage(message);
		}

        public void HandleMcpeSetScoreboardIdentity(McpeSetScoreboardIdentity message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateSoftEnum(McpeUpdateSoftEnum message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeNetworkStackLatency(McpeNetworkStackLatency message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeScriptCustomEvent(McpeScriptCustomEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleFtlCreatePlayer(FtlCreatePlayer message)
		{
			UnhandledPackage(message);
		}
	}
}
