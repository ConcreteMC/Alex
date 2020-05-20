using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Items
{
    public interface IItemRenderer : IAttachable
    {
        ResourcePackModelBase Model { get; }

        Vector3 Rotation { get; set; }
        Vector3 Translation { get; set; }
        Vector3 Scale { get; set; }
        DisplayPosition DisplayPosition { get; set; }
        DisplayElement ActiveDisplayItem { get; }

        void Update(GraphicsDevice device, ICamera camera);

        void Cache(McResourcePack pack);

        IItemRenderer Clone();
    }
}