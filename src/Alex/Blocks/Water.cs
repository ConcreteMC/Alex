using Alex.Graphics.Models;

namespace Alex.Blocks
{
    public class Water : Block
    {
        public Water(byte meta = 0) : base(8, meta)
        {
            Solid = false;
            Transparent = true;

	        BlockModel = new WaterModel();
        }
    }
}
