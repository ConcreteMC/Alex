using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Common.Blocks.Properties;
using Alex.Graphics.Models.Blocks;

namespace Alex.Blocks.State
{
    public sealed class BlockStateVariantMapper
    {
        private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockStateVariantMapper));
        private HashSet<BlockState> Variants { get; }
        private HashSet<string> _knownKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private BlockState _default = null;
        
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
        
        public BlockStateVariantMapper(List<BlockState> variants)
        {
            Variants = new HashSet<BlockState>(variants);

            foreach (var variant in variants)
            {
                variant.VariantMapper = this;

                if (variant.Default)
                    _default = variant;
            }

            foreach (var key in variants.SelectMany(x => x.States.Select(s => s.Name).Distinct()))
                _knownKeys.Add(key);
        }
		
        public bool TryResolve<T>(BlockState source, StateProperty<T> property, T value, out BlockState result)
        {
            if (!_knownKeys.Contains(property.Name))
            {
                result = source;
                return false;
            }
            
            var clone = new BlockState
            {
                Name = source.Name,
                Id = source.Id, 
                Block = source.Block,
                VariantMapper = source.VariantMapper,
                Default = source.Default,
                ModelData = source.ModelData,
                States = new HashSet<IStateProperty>(source.States.Count, source.States.Comparer)
            };
            
            foreach (var prop in source.States)
            {
                var p = prop;

                if (p.Identifier == property.Identifier)
                {
                    clone.States.Add(p.WithValue(value));
                }
                else
                {
                    clone.States.Add(p);
                }
            }
            
            if (Variants.TryGetValue(clone, out var actualValue))
            {
                result = actualValue;
                return true;
            }

            result = source;
            return false;
        }
        
        public bool TryResolve(BlockState source, string property, string value, out BlockState result)
        {
            if (!_knownKeys.Contains(property))
            {
                result = source;
                return false;
            }
            
            var propHah = property.GetHashCode(StringComparison.OrdinalIgnoreCase);
            var clone = new BlockState
            {
                Name = source.Name,
                Id = source.Id,
                Block = source.Block,
                VariantMapper = source.VariantMapper,
                Default = source.Default,
                ModelData = source.ModelData,
                States = new HashSet<IStateProperty>(source.States.Count, source.States.Comparer)
            };
            
            foreach (var prop in source.States)
            {
                var p = prop;

                if (p.Identifier == propHah)
                {
                    clone.States.Add(p.WithValue(value));
                }
                else
                {
                    clone.States.Add(p);
                }
            }
          
            if (Variants.TryGetValue(clone, out var actualValue))
            {
                result = actualValue;
                return true;
            }

            result = source;
            return false;
        }

        public BlockState[] GetVariants()
        {
            return Variants.ToArray();
        }

        public BlockState GetDefaultState()
        {
            return _default;
            //return Variants.FirstOrDefault(x => x.Default);
        }
    }
}