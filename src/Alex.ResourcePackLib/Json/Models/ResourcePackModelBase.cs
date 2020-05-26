using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.ResourcePackLib.Json.Models.Items;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Alex.ResourcePackLib.Json.Models
{
    public class ResourcePackModelBase
    {
        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public string Namespace { get; set; }
        
        /// <summary>
        /// Loads a different model from the given path, starting in assets/minecraft/models. If both "parent" and "elements" are set, the "elements" tag overrides the "elements" tag from the previous model.
        /// 
        /// Can be set to "builtin/generated" to use a model that is created out of the specified icon. Note that only the first layer is supported, and rotation can only be achieved using block states files.
        /// </summary>
        [JsonProperty("parent")]
        public string ParentName;
        
        [JsonProperty("gui_light"), JsonConverter(typeof(StringEnumConverter))]
        public GuiLight? GuiLight;
        
        [JsonIgnore]
        private ResourcePackModelBase _parent = null;
        
        [JsonIgnore]
        public ResourcePackModelBase Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                UpdateValuesFromParent();
            }
        }

        private void UpdateValuesFromParent()
        {
            if (_parent == null) return;
		    
            if (!GuiLight.HasValue && Parent.GuiLight.HasValue)
            {
                GuiLight = Parent.GuiLight;
            }

            if (Elements.Length == 0 && _parent.Elements.Length > 0)
            {
                Elements = (BlockModelElement[]) _parent.Elements.Clone();
            }

            foreach (var kvp in _parent.Textures)
            {
                if (!Textures.ContainsKey(kvp.Key))
                {
                    Textures.Add(kvp.Key, kvp.Value);
                }
            }

            foreach (var kvp in _parent.Display)
            {
                if (!Display.ContainsKey(kvp.Key))
                {
                    Display.Add(kvp.Key, kvp.Value);
                }
            }
        }
        
        /// <summary>
        /// Contains all the elements of the model. they can only have cubic forms. If both "parent" and "elements" are set, the "elements" tag overrides the "elements" tag from the previous model.
        /// </summary>
        public BlockModelElement[] Elements { get; set; } = new BlockModelElement[0];
        
        /// <summary>
        /// Holds the textures of the model. Each texture starts in assets/minecraft/textures or can be another texture variable.
        /// </summary>
        public Dictionary<string, string> Textures { get; set; } = new Dictionary<string, string>();
        
        public Dictionary<string, DisplayElement> Display { get; set; } = new Dictionary<string, DisplayElement>();
    }
}