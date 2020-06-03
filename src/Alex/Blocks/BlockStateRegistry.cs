using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks.State;
using Alex.API.Resources;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Gamestates;
using Alex.Graphics.Models.Blocks;

namespace Alex.Blocks
{
    public class BlockStateRegistry : RegistryBase<BlockState>
    {
	    private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockStateRegistry));
	    
	    private readonly Dictionary<uint, BlockState> BlockStates = new Dictionary<uint, BlockState>();
	    private readonly Dictionary<string, BlockStateVariantMapper> BlockStateByName = new Dictionary<string, BlockStateVariantMapper>();
        public BlockStateRegistry() : base("block")
        {
            Init();
        }

        private void Init()
        {
	        IProgressReceiver progressReceiver = new SplashScreen();

	        bool replace = false;
	        
            var data = BlockData.FromJson(ResourceManager.ReadStringResource("Alex.Resources.NewBlocks.json"));
			int total = data.Count;
			int done = 0;
			int importCounter = 0;
			int multipartBased = 0;

			uint c = 0;
			foreach (var entry in data)
			{
				double percentage = 100D * ((double) done / (double) total);
				progressReceiver.UpdateProgress((int) percentage, $"Importing block models...", entry.Key);

				var variantMap = new BlockStateVariantMapper();
				var state = new BlockState
				{
					Name = entry.Key
				};

				var def = entry.Value.States.FirstOrDefault(x => x.Default);
				if (def != null && def.Properties != null)
				{
					foreach (var property in def.Properties)
					{
						state = (BlockState) state.WithPropertyNoResolve(property.Key, property.Value, false);
					}
				}
				else
				{
					if (entry.Value.Properties != null)
					{
						foreach (var property in entry.Value.Properties)
						{
							state = (BlockState) state.WithPropertyNoResolve(property.Key,
								property.Value.FirstOrDefault(), false);
						}
					}
				}

				List<BlockState> variants = new List<BlockState>();
				foreach (var s in entry.Value.States)
				{
					var id = s.ID;

					BlockState variantState = (BlockState) (state).CloneSilent();
					variantState.ID = id;
					variantState.VariantMapper = variantMap;

					if (s.Properties != null)
					{
						foreach (var property in s.Properties)
						{
							//var prop = StateProperty.Parse(property.Key);
							variantState =
								(Blocks.State.BlockState) variantState.WithPropertyNoResolve(property.Key,
									property.Value, false);
							if (s.Default)
							{
								state = (BlockState) state.WithPropertyNoResolve(property.Key, property.Value, false);
							}
						}
					}

					//	resourcePack.BlockStates.TryGetValue(entry.Key)
					if (!replace && BlockStates.TryGetValue(id, out BlockState st))
					{
						Log.Warn(
							$"Duplicate blockstate id (Existing: {st.Name}[{st.ToString()}] | New: {entry.Key}[{variantState.ToString()}]) ");
						continue;
					}

					{
						if (variantState.IsMultiPart) multipartBased++;

						variantState.Default = s.Default;
						if (!variantMap.TryAdd(variantState))
						{
							Log.Warn(
								$"Could not add variant to variantmapper! ({variantState.ID} - {variantState.Name})");
							continue;
						}

						if (!BlockStates.TryAdd(id, variantState))
						{
							if (replace)
							{
								BlockStates[id] = variantState;
								importCounter++;
							}
							else
							{
								Log.Warn(
									$"Failed to add blockstate (variant), key already exists! ({variantState.ID} - {variantState.Name})");
							}
						}
						else
						{
							importCounter++;
						}
					}

					variants.Add(variantState);
				}

				foreach (var var in variants)
				{
					var.VariantMapper = variantMap;
				}


				if (!BlockStateByName.TryAdd(state.Name, variantMap))
				{
					if (replace)
					{
						BlockStateByName[state.Name] = variantMap;
					}
					else
					{
						Log.Warn($"Failed to add blockstate, key already exists! ({state.Name})");
					}
				}

				done++;
			}

			Log.Info($"Got {multipartBased} multi-part blockstate variants!");
        }
    }
}