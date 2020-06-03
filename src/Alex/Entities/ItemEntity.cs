using System;
using Alex.API.Graphics;
using Alex.API.Network;
using Alex.Graphics.Models.Items;
using Alex.Items;
using Alex.Net;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Entities
{
    public class ItemEntity : Entity
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        public ItemEntity(World level, NetworkProvider network) : base((int) EntityType.Item, level, network)
        {
            Width = 0.25;
            Height = 0.25;
            Length = 0.25;
        }
        
        protected new IItemRenderer ItemRenderer { get; set; } = null;
        private bool CanRender { get; set; } = false;
        public virtual void SetItem(Item item)
        {
            if (item.Renderer != null)
            {
                CanRender = true;
                ItemRenderer = item.Renderer.Clone();
                ItemRenderer.DisplayPosition = DisplayPosition.Ground;
            }
            else
            {
                CanRender = false;
            }
        }

        private float _rotation = 0;
        protected bool DoRotation { get; set; } = true;
        public override void Update(IUpdateArgs args)
        {
            if (CanRender)
            {
                var offset = new Vector3(0.5f, 0.5f, 0.5f);

                if (DoRotation)
                {
                    ItemRenderer?.Update(
                        Matrix.Identity * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(-offset)
                        * Matrix.CreateRotationY(MathHelper.ToRadians(_rotation)) * Matrix.CreateTranslation(offset)
                        * Matrix.CreateTranslation((KnownPosition.ToVector3())), KnownPosition);
                }
                else
                {
                    ItemRenderer?.Update(
                        Matrix.Identity * Matrix.CreateScale(Scale)
                                        * Matrix.CreateRotationY(MathHelper.ToRadians(KnownPosition.Yaw))
                                        * Matrix.CreateTranslation(KnownPosition.ToVector3()), KnownPosition);
                }

                ItemRenderer?.Update(args.GraphicsDevice, args.Camera);
            }

            if (DoRotation)
            {
                _rotation += 45f * (float) args.GameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public override void Render(IRenderArgs renderArgs)
        {
            if (!CanRender)
                return;
            
            ItemRenderer?.Render(renderArgs);
        }
    }
}