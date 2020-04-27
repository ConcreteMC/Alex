using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Entities
{
    public interface IAttachable
    {
        void Update(PlayerLocation knownPosition);
        void Render(IRenderArgs args);
    }
}