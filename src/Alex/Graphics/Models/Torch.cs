using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models
{
    public class TorchModel : Cube
    {
        public TorchModel()
        {
            SetSize(new Vector3(0.15f, 0.65f, 0.15f));
			SetOffset(new Vector3(0.425F, 0, 0.425F));
        }
    }
}
