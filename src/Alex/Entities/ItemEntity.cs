using System;
using Alex.API.Graphics;
using Alex.API.Network;
using Alex.Graphics.Models.Items;
using Alex.Items;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Entities
{
    public class ItemEntity : Entity
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        public ItemEntity(World level, INetworkProvider network) : base((int) EntityType.Item, level, network)
        {
            Width = 0.25;
            Height = 0.25;
        }

        private IItemRenderer ItemRenderer { get; set; } = null;
        private bool CanRender { get; set; } = false;
        public void SetItem(Item item)
        {
            if (item.Renderer != null)
            {
                CanRender = true;
            }
            else
            {
                CanRender = false;
            }
            
            ItemRenderer = item.Renderer;
        }

        private float _rotation = 0;
        public override void Update(IUpdateArgs args)
        {
            if (CanRender)
            {
                ItemRenderer?.Update(Matrix.CreateRotationY(MathUtils.ToRadians(_rotation)) * Matrix.CreateTranslation((KnownPosition)));
                
                ItemRenderer?.Update(args.GraphicsDevice, args.Camera);
            }

            _rotation += 45f * (float)args.GameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Render(IRenderArgs renderArgs)
        {
            if (!CanRender)
                return;
            
            ItemRenderer?.Render(renderArgs);
        }
    }
}