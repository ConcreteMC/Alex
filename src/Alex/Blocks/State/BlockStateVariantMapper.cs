using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Graphics.Models.Blocks;

namespace Alex.Blocks.State
{
    public sealed class BlockStateVariantMapper
    {
        private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockStateVariantMapper));
        private IList<BlockState> Variants { get; } = new List<BlockState>();
        
        public  bool       IsMultiPart { get; set; } = false;
        
        private BlockModel _model = null;
        public BlockModel Model
        {
            get
            {
                return _model ?? new MissingBlockModel();
            }
            set
            {
                _model = value;
            }
        }
        
        public BlockStateVariantMapper()
        {

        }
		
        public bool TryResolve(BlockState source, string property, string value, out BlockState result, params string[] requiredMatches)
        {
            //  property = property.ToLowerInvariant();
            value = value.ToLowerInvariant();

            int highestMatch = 0;
            BlockState highest = null;

            var matching = Variants.Where(x => x.Contains(property) && x[property].Equals(value, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (matching.Length == 1)
            {
                result = matching.FirstOrDefault();
                return true;
            }
            else if (matching.Length == 0)
            {
                result = source;

                return false;
            }
            
            var copiedProperties = new Dictionary<string, string>(source, StringComparer.OrdinalIgnoreCase);
            copiedProperties[property] = value.ToString();

            foreach (var variant in matching)
            {
                bool valid = true;
                foreach (var requiredMatch in requiredMatches)
                {
                    if (!(copiedProperties.TryGetValue(requiredMatch, out string copyValue) 
                          && variant.TryGetValue(requiredMatch, out string variantValue) && copyValue == variantValue))
                    {
                        valid = false;
                        break;
                    }
                }
				
                if (!valid)
                    continue;
				
                int matches = 0;
                foreach (var copy in copiedProperties)
                {
                    if (copy.Key.Equals(property))
                        continue;
                    
                    //Check if variant value matches copy value.
                    if (variant.TryGetValue(copy.Key, out string val) && copy.Value.Equals(val, StringComparison.OrdinalIgnoreCase))
                    {
                        matches++;
                    }
                }

                /*foreach (var variantProp in variant)
                {
                    if (!source.Contains(variantProp.Key))
                    {
                        matches--;
                    }
                }*/

                if (matches == source.Count)
                {
                    highestMatch = matches;
                    highest = variant;

                    break;
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