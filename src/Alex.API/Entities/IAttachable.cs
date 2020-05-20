using Alex.API.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Entities
{
    public interface IAttachable
    {
        void Update(Matrix matrix, PlayerLocation knownPosition);
        long VertexCount { get; }
        
        void Render(IRenderArgs args);
    }
}