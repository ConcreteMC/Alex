using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Blocks.Storage.Pallete;
using Alex.Entities;
using fNbt;
using MiNET.Client;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;
using NLog;
using PlayerLocation = Alex.API.Utils.PlayerLocation;

namespace Alex.Worlds.Bedrock
{
	public class BedrockClientPacketHandler : McpeClientMessageHandlerBase
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockClientPacketHandler));

		private BedrockClient BaseClient { get; }
		public BedrockClientPacketHandler(BedrockClient client) : base(client)
		{
			BaseClient = client;
		}

		private void UnhandledPackage(Packet packet)
		{
			Log.Warn($"Unhandled bedrock packet: {packet.GetType().Name} (0x{packet.Id:X2})");
		}

		public override void HandleMcpePlayStatus(McpePlayStatus message)
		{
			base.HandleMcpePlayStatus(message);
		}

		public override void HandleMcpeText(McpeText message)
		{
			BaseClient.WorldProvider.Receive(new ChatObject(message.message));
		}

		public override void HandleMcpeSetTime(McpeSetTime message)
		{
			BaseClient.WorldReceiver?.SetTime(message.time);
		}

		public override void HandleMcpeStartGame(McpeStartGame message)
		{
			Client.EntityId = message.runtimeEntityId;
			Client.NetworkEntityId = message.entityIdSelf;
			Client.SpawnPoint = message.spawn;
			Client.CurrentLocation = new MiNET.Utils.PlayerLocation(Client.SpawnPoint, message.unknown1.X, message.unknown1.X, message.unknown1.Y);

			BaseClient.WorldReceiver?.UpdatePlayerPosition(new API.Utils.PlayerLocation(new Microsoft.Xna.Framework.Vector3(Client.SpawnPoint.X, Client.SpawnPoint.Y, Client.SpawnPoint.Z), message.unknown1.X, message.unknown1.X, message.unknown1.Y));

			Dictionary<int, BlockState> ourStates = new Dictionary<int, BlockState>();
			foreach (var blockstate in message.blockstates)
			{
				//blockstate.Value.Name;
				//blockstate.Value.Data;
				BlockFactory.GetBlockState(blockstate.Value.Name);
			}
			
			{
				var packet = McpeRequestChunkRadius.CreateObject();
				packet.chunkRadius = Client.ChunkRadius;

				Client.SendPacket(packet);
			}
		}

		public override void HandleMcpeMovePlayer(McpeMovePlayer message)
		{
			if (message.runtimeEntityId != Client.EntityId) return;
			BaseClient.WorldReceiver.UpdatePlayerPosition(new 
				PlayerLocation(message.x, message.y, message.z));

			//BaseClient.SendMcpeMovePlayer();
			//Client.CurrentLocation = new PlayerLocation(message.x, message.y, message.z);
		}


		public override void HandleMcpeAdventureSettings(McpeAdventureSettings message)
		{
			base.HandleMcpeAdventureSettings(message);
			if (BaseClient.WorldReceiver.GetPlayerEntity() is Player player)
			{
				player.CanFly = ((message.flags & 0x40) == 0x40);
				player.IsFlying = ((message.flags & 0x200) == 0x200);
			}
		}

		public override void HandleMcpeAddPlayer(McpeAddPlayer message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeAddEntity(McpeAddEntity message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeRemoveEntity(McpeRemoveEntity message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeAddItemEntity(McpeAddItemEntity message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeTakeItemEntity(McpeTakeItemEntity message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeMoveEntity(McpeMoveEntity message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeRiderJump(McpeRiderJump message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateBlock(McpeUpdateBlock message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeAddPainting(McpeAddPainting message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeExplode(McpeExplode message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeLevelSoundEvent(McpeLevelSoundEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeLevelEvent(McpeLevelEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeBlockEvent(McpeBlockEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeEntityEvent(McpeEntityEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeMobEffect(McpeMobEffect message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateAttributes(McpeUpdateAttributes message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeInventoryTransaction(McpeInventoryTransaction message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeMobEquipment(McpeMobEquipment message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeMobArmorEquipment(McpeMobArmorEquipment message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeInteract(McpeInteract message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeHurtArmor(McpeHurtArmor message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetEntityData(McpeSetEntityData message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetEntityMotion(McpeSetEntityMotion message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetEntityLink(McpeSetEntityLink message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetHealth(McpeSetHealth message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetSpawnPosition(McpeSetSpawnPosition message)
		{
			Client.SpawnPoint = new Vector3(message.coordinates.X, message.coordinates.Y, message.coordinates.Z);
			Client.LevelInfo.SpawnX = (int)Client.SpawnPoint.X;
			Client.LevelInfo.SpawnY = (int)Client.SpawnPoint.Y;
			Client.LevelInfo.SpawnZ = (int)Client.SpawnPoint.Z;

			
			//		Client.SpawnPoint = new Vector3(message.);
		}

		public override void HandleMcpeAnimate(McpeAnimate message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeContainerOpen(McpeContainerOpen message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeContainerClose(McpeContainerClose message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpePlayerHotbar(McpePlayerHotbar message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeInventoryContent(McpeInventoryContent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeInventorySlot(McpeInventorySlot message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeContainerSetData(McpeContainerSetData message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeCraftingData(McpeCraftingData message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeCraftingEvent(McpeCraftingEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeGuiDataPickItem(McpeGuiDataPickItem message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeBlockEntityData(McpeBlockEntityData message)
		{
			UnhandledPackage(message);
		}
		enum PalleteType : byte
		{
			Paletted1 = 1,   // 32 blocks per word, max 2 unique blockstates
			Paletted2 = 2,   // 16 blocks per word, max 4 unique blockstates
			Paletted3 = 3,   // 10 blocks and 2 bits of padding per word, max 8 unique blockstates
			Paletted4 = 4,   // 8 blocks per word, max 16 unique blockstates
			Paletted5 = 5,   // 6 blocks and 2 bits of padding per word, max 32 unique blockstates
			Paletted6 = 6,   // 5 blocks and 2 bits of padding per word, max 64 unique blockstates
			Paletted8  = 8,  // 4 blocks per word, max 256 unique blockstates
			Paletted16 = 16, // 2 blocks per word, max 65536 unique blockstates
		}

		public override void HandleMcpeFullChunkData(McpeFullChunkData message)
		{
			using (MemoryStream stream = new MemoryStream(message.chunkData))
			{
				NbtBinaryReader defStream = new NbtBinaryReader(stream, true);

				int count = defStream.ReadByte();
				if (count < 1)
				{
					Log.Warn("Nothing to read");
					return;
				}

				Log.Debug($"Reading {count} sections");

				ChunkColumn chunkColumn = new ChunkColumn();
				chunkColumn.X = message.chunkX;
				chunkColumn.Z = message.chunkZ;

				for (int s = 0; s < count; s++)
				{
					var section = chunkColumn.Sections[s];
					byte version = defStream.ReadByte();
					int storages = 1;

					if (version == 8) //Versioon
					{
						storages = defStream.ReadByte();
					}

					if (version == 1 || version == 8)
					{
						for (int si = 0; si < storages; si++)
						{
							int paletteAndFlag = defStream.ReadByte();
							bool isRuntime = (paletteAndFlag & 1) != 0;
							int bitsPerBlock = paletteAndFlag >> 1;
							int blocksPerWord = (int)Math.Floor(32D / bitsPerBlock);
							int wordCount = (int)Math.Ceiling(4096.0 / blocksPerWord);
							long blockIndex = defStream.BaseStream.Position;
							defStream.Skip(wordCount * 4); //4 bytes per word.

							uint[] pallete;
							if (isRuntime)
							{
								int palleteSize = defStream.ReadVarInt();
								pallete = new uint[palleteSize];
								for (int pi = 0; pi < pallete.Length; pi++)
								{
									pallete[pi] = (uint)defStream.ReadVarInt();
								}
							}
							else
							{
								pallete = new uint[defStream.ReadInt32()];
								for (int palletId = 0; palletId < pallete.Length; palletId++)
								{
									NbtTagType t = defStream.ReadTagType();
								//	pallete[palletId] = 
								}
							}

							long afterPaletteIndex = defStream.BaseStream.Position;
							defStream.BaseStream.Position = blockIndex;
							int position = 0;
							for (int wordi = 0; wordi < wordCount; wordi++)
							{
								int word = defStream.ReadInt32();
								for (int block = 0; block < blocksPerWord; block++)
								{
									int state = (word >> ((position % blocksPerWord) * bitsPerBlock)) & ((1 << bitsPerBlock) - 1);
									int x = (position >> 8) & 0xF;
									int y = position & 0xF;
									int z = (position >> 4) & 0xF;
								//	MiNET.Blockstate state = MiNET.Blocks.BlockFactory.g
									section.Set(x,y,z, BlockFactory.GetBlockState(pallete[state]));

									position++;
								}
							}
							defStream.BaseStream.Position = afterPaletteIndex;
						}
					}
					else
					{
						byte[] blockIds = new byte[4096];
						defStream.Read(blockIds, 0, blockIds.Length);

						NibbleArray data = new NibbleArray(4096);
						defStream.Read(data.Data, 0, data.Data.Length);

						for (int x = 0; x < 16; x++)
						{
							for (int z = 0; z < 16; z++)
							{
								for (int y = 0; y < 16; y++)
								{
									int idx = (y << 8) + (z << 4) + x;
									
									var state = BlockFactory.GetBlockStateID(blockIds[idx], data[idx]);
									section.Set(x,y,z, BlockFactory.GetBlockState(state));
								}
							}
						}
					}
				}

				//if (stream.Position >= stream.Length - 1) continue;


				byte[] ba = new byte[512];
				if (defStream.Read(ba, 0, 256 * 2) != 256 * 2) Log.Error($"Out of data height");

				Buffer.BlockCopy(ba, 0, chunkColumn.Height, 0, ba.Length);
				//Log.Debug($"Heights:\n{Package.HexDump(ba)}");

				//if (stream.Position >= stream.Length - 1) continue;
				byte[] biomeIds = new byte[256];
				if (defStream.Read(biomeIds, 0, 256) != 256) Log.Error($"Out of data biomeId");

				for (int i = 0; i < biomeIds.Length; i++)
				{
					chunkColumn.BiomeId[i] = biomeIds[i];
				}
				//Log.Debug($"biomeId:\n{Package.HexDump(chunk.biomeId)}");

				//if (stream.Position >= stream.Length - 1) continue;


				int borderBlock = VarInt.ReadInt32(stream);
				if (borderBlock != 0)
				{
					byte[] buf = new byte[borderBlock];
					int len = defStream.Read(buf, 0, borderBlock);
					Log.Warn($"??? Got borderblock {borderBlock}. Read {len} bytes");
					Log.Debug($"{Packet.HexDump(buf)}");
					for (int i = 0; i < borderBlock; i++)
					{
						int x = (buf[i] & 0xf0) >> 4;
						int z = buf[i] & 0x0f;
						Log.Debug($"x={x}, z={z}");
					}
				}

				int extraCount = VarInt.ReadSInt32(stream);
				if (extraCount != 0)
				{
					//Log.Warn($"Got extradata\n{Package.HexDump(defStream.ReadBytes(extraCount*10))}");
					for (int i = 0; i < extraCount; i++)
					{
						var hash = VarInt.ReadSInt32(stream);
						var blockData = defStream.ReadInt16();
						Log.Warn($"Got extradata: hash=0x{hash:X2}, blockdata=0x{blockData:X2}");
					}
				}

				if (stream.Position < stream.Length - 1)
				{
					//Log.Debug($"Got NBT data\n{Package.HexDump(defStream.ReadBytes((int) (stream.Length - stream.Position)))}");

					while (stream.Position < stream.Length)
					{
						NbtFile file = new NbtFile() { BigEndian = false, UseVarInt = true };

						file.LoadFromStream(stream, NbtCompression.None);

						Log.Debug($"Blockentity: {file.RootTag}");
					}
				}
				if (stream.Position < stream.Length - 1)
				{
					Log.Warn($"Still have data to read\n{Packet.HexDump(defStream.ReadBytes((int)(stream.Length - stream.Position)))}");
				}

				BaseClient.ChunkReceived(chunkColumn);
			}

			
		}

		private static int GetIndex(int x, int y, int z)
		{
			return y << 8 | z << 4 | x;
		}

		public override void HandleMcpeSetCommandsEnabled(McpeSetCommandsEnabled message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetDifficulty(McpeSetDifficulty message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetPlayerGameType(McpeSetPlayerGameType message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpePlayerList(McpePlayerList message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSimpleEvent(McpeSimpleEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeTelemetryEvent(McpeTelemetryEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSpawnExperienceOrb(McpeSpawnExperienceOrb message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeClientboundMapItemData(McpeClientboundMapItemData message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeMapInfoRequest(McpeMapInfoRequest message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeRequestChunkRadius(McpeRequestChunkRadius message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeChunkRadiusUpdate(McpeChunkRadiusUpdate message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeItemFrameDropItem(McpeItemFrameDropItem message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeGameRulesChanged(McpeGameRulesChanged message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeCamera(McpeCamera message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeBossEvent(McpeBossEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeShowCredits(McpeShowCredits message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeAvailableCommands(McpeAvailableCommands message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeCommandOutput(McpeCommandOutput message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateTrade(McpeUpdateTrade message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateEquipment(McpeUpdateEquipment message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeTransfer(McpeTransfer message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpePlaySound(McpePlaySound message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeStopSound(McpeStopSound message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetTitle(McpeSetTitle message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeAddBehaviorTree(McpeAddBehaviorTree message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeStructureBlockUpdate(McpeStructureBlockUpdate message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeShowStoreOffer(McpeShowStoreOffer message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpePlayerSkin(McpePlayerSkin message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSubClientLogin(McpeSubClientLogin message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeInitiateWebSocketConnection(McpeInitiateWebSocketConnection message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetLastHurtBy(McpeSetLastHurtBy message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeBookEdit(McpeBookEdit message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeNpcRequest(McpeNpcRequest message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeModalFormRequest(McpeModalFormRequest message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeServerSettingsResponse(McpeServerSettingsResponse message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeShowProfile(McpeShowProfile message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetDefaultGameType(McpeSetDefaultGameType message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeRemoveObjective(McpeRemoveObjective message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetDisplayObjective(McpeSetDisplayObjective message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetScore(McpeSetScore message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeLabTable(McpeLabTable message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateBlockSynced(McpeUpdateBlockSynced message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeMoveEntityDelta(McpeMoveEntityDelta message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetScoreboardIdentityPacket(McpeSetScoreboardIdentityPacket message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateSoftEnumPacket(McpeUpdateSoftEnumPacket message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeNetworkStackLatencyPacket(McpeNetworkStackLatencyPacket message)
		{
			UnhandledPackage(message);
		}

		public override void HandleFtlCreatePlayer(FtlCreatePlayer message)
		{
			UnhandledPackage(message);
		}
	}
}
