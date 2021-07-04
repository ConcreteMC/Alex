using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Graphics.Models.Blocks;

namespace Alex.Blocks.State
{
    public sealed class BlockStateVariantMapper
    {
        private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockStateVariantMapper));
        private HashSet<BlockState> Variants { get; }
        
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
            }
        }
		
        public bool TryResolve<T>(BlockState source, StateProperty<T> property, T value, out BlockState result)
        {
            //var clone = source.WithPropertyNoResolve(property, value, true);
            //var propHah = property.GetHashCode(StringComparison.InvariantCultureIgnoreCase);
            var clone = new BlockState
            {
                Name = source.Name,
                ID = source.ID, 
                //   States = new HashSet<StateProperty>(new StatePropertyComparer()),
                Block = source.Block,
                VariantMapper = source.VariantMapper,
                Default = source.Default,
                ModelData = source.ModelData
            };
            
            //  List<StateProperty> properties = new List<StateProperty>();
            foreach (var prop in source.States)
            {
                var p = prop;

                if (p.Identifier == property.Identifier)
                {
                    clone.States.Add(property.WithValue(value));
                }
                else
                {
                    clone.States.Add(p);
                }
            }

            // clone.States = new HashSet<StateProperty>(properties, new StatePropertyComparer());
            
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
            //var clone = source.WithPropertyNoResolve(property, value, true);
            var propHah = property.GetHashCode(StringComparison.InvariantCultureIgnoreCase);
            var clone = new BlockState
            {
                Name = source.Name,
                ID = source.ID, 
             //   States = new HashSet<StateProperty>(new StatePropertyComparer()),
                Block = source.Block,
                VariantMapper = source.VariantMapper,
                Default = source.Default,
                ModelData = source.ModelData
            };
            
          //  List<StateProperty> properties = new List<StateProperty>();
            foreach (var prop in source.States)
            {
                var p = prop;

                if (p.Identifier == propHah)
                {
                    p = p.WithValue(value);
                }
                
                clone.States.Add(p);
            }

           // clone.States = new HashSet<StateProperty>(properties, new StatePropertyComparer());
            
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
            return Variants.FirstOrDefault(x => x.Default);
        }
    }
}