using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RocketUI.IO.Serialization;

namespace RocketUI.Styling
{
    public class StyleManager
    {

        //private StyleCollection _styles = new StyleCollection();


        public StyleManager()
        {

        }

        
        private static readonly Dictionary<string, Type> TypeCache = GetAllElementTypes();
        private static Dictionary<string, Type> GetAllElementTypes()
        {
            var dictionary = new Dictionary<string, Type>();

            // Get all Classes / Interfaces from all loaded assemblies. We don't care about primitives/enums etc right now
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Where(type => type.IsClass || type.IsInterface));

            var rootElementType = typeof(IVisualElement);

            foreach (var type in allTypes)
            {
                if (!rootElementType.IsAssignableFrom(type))
                    continue;

                dictionary.Add((type.Namespace ?? "") + type.Name, type);
            }

            return dictionary;
        }

        public static bool TryResolveType(string typeName, out Type type)
        {
            while (true)
            {
                if (TypeCache.TryGetValue(typeName, out type)) return true;

                var indexOf = typeName.IndexOf('.');
                if (indexOf == -1 || typeName.Length <= indexOf)
                {
                    return false;
                }

                typeName = typeName.Substring(indexOf + 1);
            }
        }

    }
}
