using Alex.CoreRT.Gamestates.Playing;

namespace Alex.CoreRT.Blocks
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
