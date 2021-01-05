using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Items
{
    public interface IItemRenderer
    {
        ResourcePackModelBase Model { get; }

        Vector3         Rotation          { get; set; }
        Vector3         Translation       { get; set; }
        Vector3         Scale             { get; set; }
        DisplayPosition DisplayPosition   { get; set; }
        DisplayElement  ActiveDisplayItem { get; set; }
        Color           DiffuseColor      { get; set; }

        void Update(GraphicsDevice device, ICamera camera);
        void Update(IUpdateArgs args, Matrix characterMatrix, Vector3 diffuseColor, PlayerLocation modelLocation);

        void Render(IRenderArgs args, bool mock, out int vertices);

        void Cache(McResourcePack pack);

        IItemRenderer Clone();
    }
}