using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex.Graphics.Items;
using Alex.Rendering;
using Alex.Utils;
using Microsoft.Xna.Framework;
using ResourcePackLib.Json.BlockStates;

namespace Alex.Graphics.Models
{
    public class ResourcePackModel : Model
    {
        private BlockStateModel Variant { get; }
        public ResourcePackModel(BlockStateModel variant)
        {
            Variant = variant;
        }

        public override VertexPositionNormalTextureColor[] GetShape(World world, Vector3 position, Block baseBlock)
        {

            foreach (var element in Variant.Model.Elements)
            {
                var from = element.From;
                var to = element.To;

                
            }
            return base.GetShape(world, position, baseBlock);
        }
    }
}
