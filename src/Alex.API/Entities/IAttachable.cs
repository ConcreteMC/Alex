using Alex.API.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.API.Entities
{
    public interface IAttachable
    {
        void Update(Vector3 attachementPoint, float positionYaw);
        void Render(IRenderArgs args);
    }
}