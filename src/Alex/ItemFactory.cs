using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models.Items;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace Alex
{
    public static class ItemFactory
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ItemFactory));

        private static Dictionary<string, ItemMapper> _javaItemMappers = new Dictionary<string, ItemMapper>();
		private static ResourceManager ResourceManager { get; set; }
		private static McResourcePack ResourcePack { get; set; }
	    public static void Init(ResourceManager resources, McResourcePack resourcePack)
	    {
		    ResourceManager = resources;
		    ResourcePack = resourcePack;

		    _javaItemMappers = JsonConvert.DeserializeObject<Dictionary<string, ItemMapper>>(Resources.Items);

	    }

	    public static bool ResolveItemTexture(string itemName, out Texture2D texture)
	    {
		    if (ResourcePack.ItemModels.TryGetValue(itemName, out ResourcePackItem item))
		    {
			    var texture0 = item.Textures.FirstOrDefault();
			    if (texture0.Value != null)
			    {
				    if (ResourcePack.TryGetTexture(texture0.Value, out texture))
				    {
					    return true;
                    }
				    else
				    {
						Log.Debug($"Could not find texture for item: {itemName} (Search Term: {texture0.Value})");
				    }
			    }
            }
		    else
		    {
			    if (ResourcePack.TryGetBlockModel(itemName, out var b))
			    {
				    var texture0 = b.Textures.OrderBy(x => x.Value.Contains("side")).FirstOrDefault();
				    if (texture0.Value != null)
				    {
					    if (ResourcePack.TryGetTexture(texture0.Value, out texture))
					    {
						    return true;
					    }
					    else
					    {
						    Log.Debug($"Could not find texture for item: {itemName} (Search Term: {texture0.Value})");
					    }
				    }
                }
			    else
			    {
				    Log.Debug($"Could not find model for item: {itemName}");
                }
            }

		    texture = null;
		    return false;
	    }

	    public static bool ResolveItemName(int protocolId, out string res)
	    {
		    var result = _javaItemMappers.FirstOrDefault(x => x.Value.ProtocolId == protocolId).Key;
		    if (result != null)
		    {
			    res = result;
			    return true;
		    }

		    res = null;
		    return false;
	    }

	    private class ItemMapper
	    {
			[JsonProperty("protocol_id")]
			public int ProtocolId { get; set; }
	    }
    }
}
