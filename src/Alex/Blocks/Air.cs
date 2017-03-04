using Alex.Gamestates.Playing;
using Alex.Graphics.Items;

namespace Alex.Blocks
{
    public class Air : Block
    {
        public Air() : base(0,0)
        {
            Solid = false;
	        Drag = CameraComponent.DefaultDrag;
        }
    }
}
