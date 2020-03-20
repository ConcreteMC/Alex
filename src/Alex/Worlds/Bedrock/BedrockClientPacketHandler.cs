using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
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
using Alex.API.Events;
using Alex.API.Events.World;
using Alex.API.Network.Bedrock;
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
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using fNbt;
using Jose;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MiNET.Client;
using MiNET.Net;
using MiNET.Utils;
using Newtonsoft.Json;
using NLog;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using BitArray = System.Collections.BitArray;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using BlockState = MiNET.Utils.IBlockState;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using Inventory = Alex.Utils.Inventory;
using NibbleArray = MiNET.Utils.NibbleArray;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Worlds.Bedrock
{
	public class BedrockClientPacketHandler : IMcpeClientMessageHandler
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockClientPacketHandler));

		private IBedrockNetworkProvider Client { get; }
		private Alex AlexInstance { get; }
        private CancellationToken CancellationToken { get; }
        private ChunkProcessor ChunkProcessor { get; }

        private WorldProvider WorldProvider { get; }
        private PlayerProfile PlayerProfile { get; }
        private IEventDispatcher EventDispatcher { get; }
        public BedrockClientPacketHandler(IBedrockNetworkProvider client, IEventDispatcher eventDispatcher, WorldProvider worldProvider, PlayerProfile profile, Alex alex, CancellationToken cancellationToken) //:
	       // base(client)
        {
	        EventDispatcher = eventDispatcher;
	        Client = client;
	        AlexInstance = alex;
	        CancellationToken = cancellationToken;
	        WorldProvider = worldProvider;
	        PlayerProfile = profile;
	        
	        AnvilWorldProvider.LoadBlockConverter();

	        ChunkProcessor = new ChunkProcessor(4,
		        alex.Services.GetRequiredService<IOptionsProvider>().AlexOptions.MiscelaneousOptions.ServerSideLighting,
		        cancellationToken);
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

	        ECPublicKeyParameters remotePublicKey = (ECPublicKeyParameters) PublicKeyFactory.CreateKey(x5u.DecodeBase64());

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

		        Client.InitiateEncryption(Base64Url.Decode(x5u), Base64Url.Decode(data.salt));
	        }
	        catch (Exception e)
	        {
		        Log.Error(e, token);
		        throw;
	        }
        }

        public void HandleMcpePlayStatus(McpePlayStatus message)
		{
			Client.PlayerStatus = message.status;
			
			if (Client.PlayerStatus == 3)
			{
				Client.HasSpawned = true;

				Client.PlayerStatusChanged.Set();

				if (Client is MiNetClient miNetClient)
				{
					miNetClient.IsEmulator = false;
				}

				Client.SendMcpeMovePlayer();
				
				var packet = McpeSetLocalPlayerAsInitializedPacket.CreateObject();
				packet.runtimeEntityId =  Client.WorldReceiver.GetPlayerEntity().EntityId;
				 Client.SendPacket(packet);
			}
		}

        public void HandleMcpeDisconnect(McpeDisconnect message)
        {
            Log.Info($"Received disconnect: {message.message}");
            Client.ShowDisconnect(message.message, false);
            
           // Client.
           // base.HandleMcpeDisconnect(message);
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
			EventDispatcher.DispatchEvent(new ChatMessageReceivedEvent(new ChatObject(message.message)));
		}

		public void HandleMcpeSetTime(McpeSetTime message)
		{
			Client.WorldReceiver?.SetTime(message.time);
			_changeDimensionResetEvent.Set();
		}

		private IReadOnlyDictionary<uint, BlockStateContainer> _blockStateMap;
		public void HandleMcpeStartGame(McpeStartGame message)
		{
			Client.EntityId = message.runtimeEntityId;
			Client.NetworkEntityId = message.entityIdSelf;
			Client.SpawnPoint = message.spawn;
			Client.CurrentLocation = new MiNET.Utils.PlayerLocation(Client.SpawnPoint, message.spawn.X, message.spawn.X, message.spawn.Y);

			Client.WorldReceiver?.UpdatePlayerPosition(new API.Utils.PlayerLocation(new Microsoft.Xna.Framework.Vector3(Client.SpawnPoint.X, Client.SpawnPoint.Y, Client.SpawnPoint.Z), message.spawn.X, message.spawn.X, message.spawn.Y));

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
			
			_blockStateMap = ourStates;
			ChunkProcessor._blockStateMap = ourStates;
			
			//File.WriteAllText("blockies.json", JsonConvert.SerializeObject(message.blockstates, Formatting.Indented));
			{
				Client.RequestChunkRadius(Client.ChunkRadius);
			}

            if (Client.WorldReceiver?.GetPlayerEntity() is Player player)
            {
                player.Inventory.IsPeInventory = true;
            }
        }

		public void HandleMcpeMovePlayer(McpeMovePlayer message)
		{
			if (message.runtimeEntityId != Client.EntityId)
			{
				Client.WorldReceiver.UpdateEntityPosition(message.runtimeEntityId, 
					new PlayerLocation(message.x, message.y - Player.EyeLevel, message.z, message.headYaw, message.yaw, message.pitch));
				return;
			}
			
			Client.WorldReceiver.UpdatePlayerPosition(new 
				PlayerLocation(message.x, message.y, message.z));

			// Client.SendMcpeMovePlayer();
			Client.CurrentLocation = new MiNET.Utils.PlayerLocation(message.x, message.y, message.z);
			Client.SendMcpeMovePlayer();
		}


		public void HandleMcpeAdventureSettings(McpeAdventureSettings message)
		{
			//Client.UserPermission = (CommandPermission) message.commandPermission;
			
			//base.HandleMcpeAdventureSettings(message);
			if (Client.WorldReceiver.GetPlayerEntity() is Player player)
			{
				player.CanFly = ((message.flags & 0x40) == 0x40);
				player.IsFlying = ((message.flags & 0x200) == 0x200);
			}
		}

		private ConcurrentDictionary<MiNET.Net.UUID, PlayerMob> _players = new ConcurrentDictionary<MiNET.Net.UUID, PlayerMob>();
        public void HandleMcpeAddPlayer(McpeAddPlayer message)
		{
		//	UUID u = new UUID(message.uuid.GetBytes());
			if (_players.TryGetValue(message.uuid, out PlayerMob mob))
			{
				//MiNET.Player
				mob.EntityId = message.runtimeEntityId;
				mob.KnownPosition = new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch);

				foreach (var meta in message.metadata.GetValues())
				{
					switch ((MiNET.Entities.Entity.MetadataFlags) meta.Identifier)
					{
						case MiNET.Entities.Entity.MetadataFlags.CollisionBoxHeight:
						{
							if (meta is MetadataFloat flt)
							{
								mob.Height = flt.Value;
							}
						} break;
						case MiNET.Entities.Entity.MetadataFlags.CollisionBoxWidth:
						{
							if (meta is MetadataFloat fltw)
							{
								mob.Width = fltw.Value;
							}
						} break;
						case MiNET.Entities.Entity.MetadataFlags.Scale:
						{
							if (meta is MetadataFloat flt)
							{
								mob.Scale = flt.Value;
							}
						} break;
						case MiNET.Entities.Entity.MetadataFlags.EntityFlags:
						{
							if (meta is MetadataLong lng)
							{
								BitArray bits = new BitArray(BitConverter.GetBytes(lng.Value));
								mob.IsInvisible = bits[(int) MiNET.Entities.Entity.DataFlags.Invisible];
							}
						}
							break;
					}
				}
				//mob.Height = message.metadata[(int) MiNET.Entities.Entity.MetadataFlags.CollisionBoxHeight]
				
				if (Client.WorldReceiver is World w)
				{
					mob.IsSpawned = true;
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
			if (!Directory.Exists("skins"))
				Directory.CreateDirectory("skins");
			
			if (message.records is PlayerAddRecords addRecords)
			{
				foreach (var r in addRecords)
				{
					var u = new API.Utils.UUID(r.ClientUuid.GetBytes());
					if (_players.ContainsKey(r.ClientUuid)) continue;

					Texture2D skinTexture;
					if (r.Skin.TryGetBitmap(out Bitmap skinBitmap))
					{
						skinTexture =
							TextureUtils.BitmapToTexture2D(AlexInstance.GraphicsDevice, skinBitmap);
					}
					else
					{
						AlexInstance.Resources.ResourcePack.TryGetBitmap("entity/alex", out Bitmap rawTexture);
						skinTexture = TextureUtils.BitmapToTexture2D(AlexInstance.GraphicsDevice, rawTexture);
					}

					Client.WorldReceiver?.AddPlayerListItem(new PlayerListItem(u, r.DisplayName, Gamemode.Survival, 0));

					EntityModelRenderer renderer = null;
					
					if (!string.IsNullOrWhiteSpace(r.Skin.GeometryData) && r.Skin.GeometryData != "null")
					{
						try
						{
							BedrockGeometry geo =
								JsonConvert.DeserializeObject<BedrockGeometry>(r.Skin.GeometryData, new SingleOrArrayConverter<Vector3>(), new SingleOrArrayConverter<Vector2>(), new Vector3Converter(), new Vector2Converter());

							if (geo != null && geo.MinecraftGeometry != null)
							{
								//var abc = geo.MinecraftGeometry.FirstOrDefault()
								var modelRenderer =
									new EntityModelRenderer(geo.MinecraftGeometry.FirstOrDefault(x => x.Description.Identifier == r.Skin.SkinResourcePatch.Geometry.Default), skinTexture);

								if (modelRenderer.Valid)
								{
									renderer = modelRenderer;
								}
								else
								{
									//modelRenderer.Dispose();
									
									var path = Path.Combine("skins", $"invalid-{r.Skin.SkinId}.json");
									if (!File.Exists(path))
									{
										File.WriteAllText(path, JsonConvert.SerializeObject(r.Skin, Formatting.Indented));
									}
								}

								//m.ModelRenderer =
								//	new EntityModelRenderer(geo.MinecraftGeometry.FirstOrDefault(), skinTexture);
							}
							else if (geo == null)
							{
								//Log.Info($"Geometry was null:");

								var path = Path.Combine("skins", $"{r.Skin.SkinId}.json");
								if (!File.Exists(path))
								{
									File.WriteAllText(path, JsonConvert.SerializeObject(r.Skin, Formatting.Indented));
								}
							}
						}
						catch (Exception ex)
						{
							Log.Warn(ex, $"Could not create geometry: {ex.ToString()}");
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
							out Bitmap bmp))
						{
							Texture2D t = TextureUtils.BitmapToTexture2D(AlexInstance.GraphicsDevice, bmp);

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


			WorldProvider.SpawnEntity(entityId, entity);
		}


		public void HandleMcpeAddEntity(McpeAddEntity message)
		{
			var type = message.entityType.Replace("minecraft:", "");
            if (Enum.TryParse(typeof(EntityType), type, true, out object res))
            {
                SpawnMob(message.runtimeEntityId, Guid.NewGuid(), (EntityType)res, new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch), new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ));
            }
            else
            {
                Log.Warn($"Unknown mob: {type}");
            }
        }

		public void HandleMcpeRemoveEntity(McpeRemoveEntity message)
		{
			WorldProvider.DespawnEntity(message.entityIdSelf);
			// Client.WorldReceiver?.DespawnEntity(message.entityIdSelf);
		}

		public void HandleMcpeAddItemEntity(McpeAddItemEntity message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeTakeItemEntity(McpeTakeItemEntity message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMoveEntity(McpeMoveEntity message)
		{
			if (message.runtimeEntityId != Client.EntityId)
			{
                 Client.WorldReceiver.UpdateEntityPosition(message.runtimeEntityId,
					new PlayerLocation(message.position.X, message.position.Y, message.position.Z, 
						message.position.HeadYaw, message.position.Yaw, -message.position.Pitch));
				return;
			}

           //  Client.WorldReceiver.UpdateEntityPosition(message.runtimeEntityId, new PlayerLocation(message.position), true, true, true);
			UnhandledPackage(message);
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
/*
				if ((message.flags & McpeMoveEntityDelta.HasX) != 0)
				{
					startPosition.X = 256;
				}
			
				if ((message.flags & McpeMoveEntityDelta.HasY) != 0)
				{
					startPosition.Y = 256;
				}
			
				if ((message.flags & McpeMoveEntityDelta.HasZ) != 0)
				{
					startPosition.Z = 256;
				}
			*/

				/*var relative = endPosition.ToVector3() - startPosition.ToVector3();
				relative.X = float.IsNaN(relative.X) ? 0 : relative.X;
				relative.Y = float.IsNaN(relative.Y) ? 0 : relative.Y;
				relative.Z = float.IsNaN(relative.Z) ? 0 : relative.Z;*/

				/*Client.WorldReceiver.UpdateEntityPosition(message.runtimeEntityId,
					new PlayerLocation(relative.X, relative.Y, relative.Z, message.currentPosition.HeadYaw,
						message.currentPosition.Yaw, message.currentPosition.Pitch)
					{
						OnGround = message.isOnGround
					}, true, updateHeadYaw || updateYaw, updatePitch);*/
				
				//	Log.Info($"Rel: {relative}");
				
				if (Client.WorldReceiver.TryGetEntity(message.runtimeEntityId, out var entity))
				{
					var known = entity.KnownPosition;
					
					MiNET.Utils.PlayerLocation startPosition = new MiNET.Utils.PlayerLocation(known.X, known.Y, known.Z, known.HeadYaw, known.Yaw, known.Pitch);
					
					var endPosition = message.GetCurrentPosition(startPosition);
					
					endPosition.X = float.IsNaN(endPosition.X) ? known.X : endPosition.X;
					endPosition.Y = float.IsNaN(endPosition.Y) ? known.Y : endPosition.Y;
					endPosition.Z = float.IsNaN(endPosition.Z) ? known.Z : endPosition.Z;
					
					Log.Info($"Distance: {endPosition.DistanceTo(startPosition)} | Delta: {endPosition - startPosition.ToVector3()} | Start: {startPosition} End: {endPosition}");
					
					entity.KnownPosition = new PlayerLocation(endPosition);
				}
			}
		}

		public void HandleMcpeRiderJump(McpeRiderJump message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateBlock(McpeUpdateBlock message)
		{
			//if (message.storage != 0)
			//{
			//	Log.Warn($"UPDATEBLOCK: Unsupported block storage! {message.storage}");
			//	return;
			//}
			
			if (_blockStateMap.TryGetValue(message.blockRuntimeId, out var bs))
			{
				if (ChunkProcessor.TryConvertBlockState(bs, out var state))
					 Client.WorldReceiver?.SetBlockState(
						new BlockCoordinates(message.coordinates.X, message.coordinates.Y, message.coordinates.Z), 
						state, (int) message.storage);
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
			UnhandledPackage(message);
		}

		public void HandleMcpeBlockEvent(McpeBlockEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeEntityEvent(McpeEntityEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMobEffect(McpeMobEffect message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateAttributes(McpeUpdateAttributes message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeInventoryTransaction(McpeInventoryTransaction message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMobEquipment(McpeMobEquipment message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMobArmorEquipment(McpeMobArmorEquipment message)
		{
			UnhandledPackage(message);
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
			UnhandledPackage(message);
		}

		public void HandleMcpeSetEntityMotion(McpeSetEntityMotion message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetEntityLink(McpeSetEntityLink message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetHealth(McpeSetHealth message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetSpawnPosition(McpeSetSpawnPosition message)
		{
			Client.SpawnPoint = new Vector3(message.coordinates.X, message.coordinates.Y, message.coordinates.Z);
			Client.LevelInfo.SpawnX = (int)Client.SpawnPoint.X;
			Client.LevelInfo.SpawnY = (int)Client.SpawnPoint.Y;
			Client.LevelInfo.SpawnZ = (int)Client.SpawnPoint.Z;

			
			//		Client.SpawnPoint = new Vector3(message.);
		}

		public void HandleMcpeAnimate(McpeAnimate message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeRespawn(McpeRespawn message)
		{
			Client.CurrentLocation = new MiNET.Utils.PlayerLocation(message.x, message.y, message.z);
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
				if ( Client.WorldReceiver?.GetPlayerEntity() is Player player)
				{
					inventory = player.Inventory;
				}
			}

			if (inventory == null) return;

			for (var index = 0; index < message.input.Count; index++)
			{
				var slot = message.input[index];
				
				var usedIndex = index;

                if (ItemFactory.TryGetItem(slot.Id, slot.Metadata, out Item item))
				{
                    item.Count = slot.Count;
                    inventory[usedIndex] = item;
				}
                else
                {
                    Log.Warn($"Failed to set slot: {index} Id: {slot.Id}:{slot.Metadata}");
                }
            }
		}

		public void HandleMcpeInventorySlot(McpeInventorySlot message)
		{
			Inventory inventory = null;
			if (message.inventoryId == 0x00)
			{
				if ( Client.WorldReceiver?.GetPlayerEntity() is Player player)
				{
					inventory = player.Inventory;
                    if (!inventory.IsPeInventory)
                        player.Inventory.IsPeInventory = true;
                }
			}

			if (inventory == null) return;
			
			var index = (int)message.slot;

            if (ItemFactory.TryGetItem(message.item.Id, message.item.Metadata, out Item item))
            {
                item.Count = message.item.Count;
				inventory[index] = item;
              //  Log.Info($"Set inventory slot: {message.slot} Id: {message.item.Id}:{message.item.Metadata} x {message.item.Count} Name: {item.DisplayName} IsPeInv: {inventory.IsPeInventory}");
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
				LoadingWorldState loadingWorldState = new LoadingWorldState();
				AlexInstance.GameStateManager.SetActiveState(loadingWorldState, true);
				loadingWorldState.UpdateProgress(LoadingState.LoadingChunks, 0);
				
				
				foreach (var loadedChunk in provider.LoadedChunks)
				{
					provider.UnloadChunk(loadedChunk);
				}
				
				ThreadPool.QueueUserWorkItem(async o =>
				{
					double radiusSquared = Math.Pow(Client.ChunkRadius, 2);
					var target = radiusSquared * 3;

					int percentage = 0;
					bool ready = false;
					
					do
					{
						percentage = (int) (provider.LoadedChunks.Count() / target) * 100;
						
						loadingWorldState.UpdateProgress(LoadingState.LoadingChunks,
							percentage);

						if (!ready)
						{
							if (_changeDimensionResetEvent.WaitOne(0))
								ready = true;
						}
						else
						{
							await Task.Delay(50);
						}

						if (percentage > 50)
						{
							McpePlayerAction action = McpePlayerAction.CreateObject();
							action.runtimeEntityId = Client.EntityId;
							action.actionId = (int) PlayerAction.DimensionChangeAck;
							Client.SendPacket(action);
						}
						
					} while (!ready || percentage < 99);
					
					AlexInstance.GameStateManager.Back();
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
			UnhandledPackage(message);
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
			UnhandledPackage(message);
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

		public void HandleMcpeResourcePackDataInfo(McpeResourcePackDataInfo message)
		{
			throw new NotImplementedException();
		}

		public void HandleMcpeResourcePackChunkData(McpeResourcePackChunkData message)
		{
			throw new NotImplementedException();
		}

		public void HandleMcpeTransfer(McpeTransfer message)
		{
			 Client.SendDisconnectionNotification();
			 Client.Close();
			
			IPHostEntry hostEntry = Dns.GetHostEntry(message.serverAddress);

			if (hostEntry.AddressList.Length > 0)
			{
				var ip = hostEntry.AddressList[0];
				AlexInstance.ConnectToServer(new IPEndPoint(ip, message.port), PlayerProfile, true);
			}
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
			UnhandledPackage(message);
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
