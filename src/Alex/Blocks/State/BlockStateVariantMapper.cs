using System;
using System.Collections.Generic;
using System.Linq;

namespace Alex.Blocks.State
{
    public sealed class BlockStateVariantMapper
    {
        private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockStateVariantMapper));
        private IList<BlockState> Variants { get; } = new List<BlockState>();

        public BlockStateVariantMapper()
        {

        }
		
        public bool TryResolve(BlockState source, string property, string value, bool prioritize, out BlockState result, params string[] requiredMatches)
        {
            var copiedProperties = source.ToDictionary();
            copiedProperties[property] = value.ToString();

            int highestMatch = 0;
            BlockState highest = null;

            var matching = GetVariants().Where(x =>
                (x.TryGetValue(property, out string xVal) &&
                 xVal.Equals(value, StringComparison.InvariantCultureIgnoreCase))).ToArray();

            if (matching.Length == 1)
            {
                result = matching.FirstOrDefault();
                return true;
            }

            foreach (var variant in matching)
            {
                bool valid = true;
                foreach (var requiredMatch in requiredMatches)
                {
                    if (!(copiedProperties.TryGetValue(requiredMatch, out string copyValue) && variant.TryGetValue(requiredMatch, out string variantValue) && copyValue == variantValue))
                    {
                        valid = false;
                        break;
                    }
                }
				
                if (!valid)
                    continue;
				
                int matches = 0;
                foreach (var copy in copiedProperties.Where(x => x.Key != property))
                {
                    //Check if variant value matches copy value.
                    if (variant.TryGetValue(copy.Key, out string val) && copy.Value.Equals(val, StringComparison.InvariantCultureIgnoreCase))
                    {
                        matches++;
                    }
                }

                foreach (var variantProp in variant.ToDictionary())
                {
                    if (!copiedProperties.ContainsKey(variantProp.Key))
                    {
                        matches--;
                    }
                }

                if (matches > highestMatch)
                {
                    highestMatch = matches;
                    highest = variant;
                }
            }

            if (highest != null)
            {
                result = highest;
                return true;
            }

            result = null;
            return false;
        }

        public bool TryAdd(BlockState state)
        {
            //return Variants.TryAdd(state);
            if (Variants.Contains(state)) return false;
            Variants.Add(state);
            return true;
        }

        public BlockState[] GetVariants()
        {
            return Variants.ToArray();
        }

        public BlockState GetDefaultState()
        {
            return Variants.FirstOrDefault(x => x.Default);
        }
    }
}