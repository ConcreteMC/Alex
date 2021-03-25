using System;
using System.Collections.Generic;
using fNbt;
using MiNET;
using MiNET.Net;
using MiNET.Utils;
using NLog;

namespace Alex.Net.Bedrock.Packets
{
	public class StartGame : McpeStartGame
	{
		public GameRules ReadNewGameRules()
		{
			GameRules gameRules = new GameRules();

			uint count = ReadUnsignedVarInt();
			//Log.Info($"Gamerule count: {count}");
			for (int i = 0; i < count; i++)
			{
				string name = ReadString();
				uint   type = ReadUnsignedVarInt();
				switch (type)
				{
					case 1:
					{
						GameRule<bool> rule = new GameRule<bool>(name, ReadBool());
						gameRules.Add(rule);
						break;
					}
					case 2:
					{
						GameRule<uint> rule = new GameRule<uint>(name, ReadUnsignedVarInt());
						gameRules.Add(rule);
						break;
					}
					case 3:
					{
						GameRule<float> rule = new GameRule<float>(name, ReadFloat());
						gameRules.Add(rule);
						break;
					}
				}
			}

			return gameRules;
		}
		
		/// <inheritdoc />
		protected override void DecodePacket()
		{
			//base.DecodePacket();
			//this.Id = ReadByte();
			this.Id = this.IsMcpe ? (byte) this.ReadVarInt() : this.ReadByte();
			
			entityIdSelf = ReadSignedVarLong();
			runtimeEntityId = ReadUnsignedVarLong();
			playerGamemode = ReadSignedVarInt();
			spawn = ReadVector3();
			rotation = ReadVector2();
			
			seed = ReadSignedVarInt();
			
			biomeType = ReadShort();
			biomeName = ReadString();
			dimension = ReadSignedVarInt();
			
			generator = ReadSignedVarInt();
			gamemode = ReadSignedVarInt();
			difficulty = ReadSignedVarInt();
			
			x = ReadVarInt();
			y = (int) ReadUnsignedVarInt();
			z = ReadVarInt();
			
			hasAchievementsDisabled = ReadBool();
			dayCycleStopTime = ReadSignedVarInt();
			eduOffer = ReadSignedVarInt();
			hasEduFeaturesEnabled = ReadBool();
			eduProductUuid = ReadString();
			rainLevel = ReadFloat();
			lightningLevel = ReadFloat();
			hasConfirmedPlatformLockedContent = ReadBool();
			isMultiplayer = ReadBool();
			broadcastToLan = ReadBool();
			xboxLiveBroadcastMode = ReadVarInt();
			platformBroadcastMode = ReadVarInt();
			enableCommands = ReadBool();
			isTexturepacksRequired = ReadBool();
			gamerules = ReadNewGameRules();

			//experiments
			var experimentCount = ReadInt();
			//Log.Info($"Experiment count: {experimentCount}");
			
			for (int i = 0; i < experimentCount; i++)
			{
				ReadString(); //Experiment name
				ReadBool(); //Enabled
			}
			
			unknown2 = ReadBool(); //hasPreviouslyUsedExperiments
			
			bonusChest = ReadBool();
			mapEnabled = ReadBool();
			permissionLevel = ReadSignedVarInt();
			serverChunkTickRange = ReadInt();
			hasLockedBehaviorPack = ReadBool();
			hasLockedResourcePack = ReadBool();
			isFromLockedWorldTemplate = ReadBool();
			useMsaGamertagsOnly = ReadBool();
			isFromWorldTemplate = ReadBool();
			isWorldTemplateOptionLocked = ReadBool();
			onlySpawnV1Villagers = ReadBool();
			gameVersion = ReadString();
			limitedWorldWidth = ReadInt();
			limitedWorldLength = ReadInt();
			isNewNether = ReadBool();

			if (ReadBool())
			{
				experimentalGameplayOverride = ReadBool();
			}

			levelId = ReadString();
			worldName = ReadString();
			premiumWorldTemplateId = ReadString();
			isTrial = ReadBool();
			movementType = ReadSignedVarInt();
			
			this.movementRewindHistorySize = this.ReadSignedVarInt();
			this.enableNewBlockBreakSystem = this.ReadBool();
			
			currentTick = ReadLong();
			enchantmentSeed = ReadSignedVarInt();
			
			blockPalette = ReadAlternateBlockPalette();
			itemstates = ReadItemstates();
			
			multiplayerCorrelationId = ReadString();
			enableNewInventorySystem = ReadBool();
		}

		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(StartGame));
		public BlockPalette ReadAlternateBlockPalette()
		{
			var  result = new BlockPalette();
			uint count  = ReadUnsignedVarInt();
			
			//Log.Info($"Block count startgame: {count}");
			for (int runtimeId = 0; runtimeId < count; runtimeId++)
			{
				var record = new BlockStateContainer();
				record.RuntimeId = runtimeId;
				record.Id = record.RuntimeId;
				record.Name = ReadString();
				record.States = new List<IBlockState>();
				
				
				var nbt     = NetworkUtils.ReadNewNbt(_reader);
				var rootTag = nbt.NbtFile.RootTag;

				if (rootTag is NbtList nbtList)
				{
					foreach (NbtTag tag in nbtList)
					{
						var s = tag["states"];

						if (s is NbtCompound compound)
						{
							foreach (NbtTag stateTag in compound)
							{
								IBlockState state = null;

								switch (stateTag.TagType)
								{
									case NbtTagType.Byte:
										state = new BlockStateByte() {Name = stateTag.Name, Value = stateTag.ByteValue};

										break;

									case NbtTagType.Int:
										state = new BlockStateInt() {Name = stateTag.Name, Value = stateTag.IntValue};

										break;

									case NbtTagType.String:
										state = new BlockStateString()
										{
											Name = stateTag.Name, Value = stateTag.StringValue
										};

										break;

									default:
										throw new ArgumentOutOfRangeException();
								}

								record.States.Add(state);
							}
						}
						else if (s is NbtList list)
						{
							foreach (NbtTag stateTag in list)
							{
								IBlockState state = null;

								switch (stateTag.TagType)
								{
									case NbtTagType.Byte:
										state = new BlockStateByte() {Name = stateTag.Name, Value = stateTag.ByteValue};

										break;

									case NbtTagType.Int:
										state = new BlockStateInt() {Name = stateTag.Name, Value = stateTag.IntValue};

										break;

									case NbtTagType.String:
										state = new BlockStateString()
										{
											Name = stateTag.Name, Value = stateTag.StringValue
										};

										break;

									default:
										throw new ArgumentOutOfRangeException();
								}

								record.States.Add(state);
							}
						}

						result.Add(record);
					}
				}
				else if (rootTag is NbtCompound c)
				{
					foreach (NbtTag tag in c)
					{
						var s = tag["states"];

						if (s is NbtCompound compound)
						{
							foreach (NbtTag stateTag in compound)
							{
								IBlockState state = null;
								switch (stateTag.TagType)
								{
									case NbtTagType.Byte:
										state = new BlockStateByte()
										{
											Name = stateTag.Name,
											Value = stateTag.ByteValue
										};
										break;
									case NbtTagType.Int:
										state = new BlockStateInt()
										{
											Name = stateTag.Name,
											Value = stateTag.IntValue
										};
										break;
									case NbtTagType.String:
										state = new BlockStateString()
										{
											Name = stateTag.Name,
											Value = stateTag.StringValue
										};
										break;
									default:
										throw new ArgumentOutOfRangeException();
								}
								record.States.Add(state);
							}
						}
						else if (s is NbtList list)
						{
							foreach (NbtTag stateTag in list)
							{
								IBlockState state = null;
								switch (stateTag.TagType)
								{
									case NbtTagType.Byte:
										state = new BlockStateByte()
										{
											Name = stateTag.Name,
											Value = stateTag.ByteValue
										};
										break;
									case NbtTagType.Int:
										state = new BlockStateInt()
										{
											Name = stateTag.Name,
											Value = stateTag.IntValue
										};
										break;
									case NbtTagType.String:
										state = new BlockStateString()
										{
											Name = stateTag.Name,
											Value = stateTag.StringValue
										};
										break;
									default:
										throw new ArgumentOutOfRangeException();
								}
								record.States.Add(state);
							}
						}

						result.Add(record);
					}
				}
			}
			return result;
		}
	}
}