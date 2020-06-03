using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Models.Entities;

namespace ResourceConverterCore.Converter
{
    public static class ResourceConverterContext
    {
        
        public static IReadOnlyDictionary<string, EntityModel> EntityModels { get; set; }
        public static string CurrentModelName { get; set; }
        public static EntityModel CurrentModel { get; set; }

    }
}
