using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Entities
{
    public interface IAttachable
    {
        bool ApplyHeadYaw { get; set; }
        bool ApplyPitch { get; set; }

        void Update(IUpdateArgs args, Matrix characterMatrix, Vector3 diffuseColor, PlayerLocation modelLocation);

        void Render(IRenderArgs args, bool mock, out int vertices);
        
        string Name { get; }
    }
}