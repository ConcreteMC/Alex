using Alex.API.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.API.Entities
{
    public interface IAttachable
    {
        long VertexCount { get; }
        
        void Update(Matrix parentMatrix);
        void Render(IRenderArgs args);
    }
}