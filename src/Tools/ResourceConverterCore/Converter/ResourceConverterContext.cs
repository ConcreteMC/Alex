using System;
using System.Collections.Generic;
using System.Text;
using Alex.ResourcePackLib.Json.Models.Entities;

namespace ResourceConverter
{
    public static class ResourceConverterContext
    {
        
        public static IReadOnlyDictionary<string, EntityModel> EntityModels { get; set; }
        public static string CurrentModelName { get; set; }
        public static EntityModel CurrentModel { get; set; }

    }
}
