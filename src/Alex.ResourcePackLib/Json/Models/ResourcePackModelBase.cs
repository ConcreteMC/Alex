using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Alex.ResourcePackLib.Json.Models.Items;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Alex.ResourcePackLib.Json.Models
{
    public enum ModelType
    {
        Item,
        Block,
        Unknown
    }
    
    public class ResourcePackModelBase
    {
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
                UpdateValuesFromParent(value);
            }
        }

        [JsonIgnore] public ModelType Type { get; internal set; } = ModelType.Unknown;
        
        private void UpdateValuesFromParent(ResourcePackModelBase parent)
        {
            if (parent == null) return;
		    
            if (!GuiLight.HasValue && parent.GuiLight.HasValue)
            {
                GuiLight = parent.GuiLight;
            }

            if (Elements.Length == 0 && parent.Elements.Length > 0)
            {
                Elements = (ModelElement[]) parent.Elements.Clone();
            }

            foreach (var kvp in parent.Textures)
            {
                Textures.TryAdd(kvp.Key, kvp.Value);
            }

            foreach (var kvp in parent.Display)
            {
                Display.TryAdd(kvp.Key, kvp.Value);
            }
        }
        
        /// <summary>
        /// Contains all the elements of the model. they can only have cubic forms. If both "parent" and "elements" are set, the "elements" tag overrides the "elements" tag from the previous model.
        /// </summary>
        public ModelElement[] Elements { get; set; } = new ModelElement[0];
        
        /// <summary>
        /// Holds the textures of the model. Each texture starts in assets/minecraft/textures or can be another texture variable.
        /// </summary>
        public Dictionary<string, string> Textures { get; set; } = new Dictionary<string, string>();
        
        public Dictionary<string, DisplayElement> Display { get; set; } = new Dictionary<string, DisplayElement>();
        
        /// <summary>
        /// Whether to use ambient occlusion (true - default), or not (false).
        /// </summary>
        public bool AmbientOcclusion { get; set; } = true;
    }
}