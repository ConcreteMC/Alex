using Alex.ResourcePackLib.Json.Models.Entities;

namespace Alex.Entities
{
        public static class ModelFactory
        {
            public static bool TryGetModel(string geometry, out EntityModel model)
            {
				if (Alex.Instance.Resources.BedrockResourcePack.EntityModels.TryGetValue(geometry, out var m))
				{
					model = m;
					return true;
				}

				model = null;
				return false;
            }
        }
}