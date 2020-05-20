using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Properties;
using Alex.Entities;
using Alex.Worlds;

namespace Alex.Blocks.Minecraft
{
    public class Trapdoor : Block
    {
        private static PropertyBool OPEN = new PropertyBool("open");
        private static PropertyBool HALF = new PropertyBool("half", "top", "bottom");
        private static PropertyFace FACING = new PropertyFace("facing");

        public Trapdoor() : base()
        {
            Solid = true;
            IsFullCube = false;
            Transparent = true;
            CanInteract = true;
        }

    }
}