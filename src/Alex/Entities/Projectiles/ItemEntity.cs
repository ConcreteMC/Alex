using Alex.API.Graphics;
using Alex.Graphics.Models.Items;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Java;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Entities.Projectiles
{
    public class ItemEntity : Entity
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        public ItemEntity(World level, NetworkProvider network) : base((int) EntityType.Item, level, network)
        {
            Width = 0.25;
            Height = 0.25;
          //  Length = 0.25;
            
            Gravity = 0.04;
            Drag = 0.02;
        }

        /// <inheritdoc />
        protected override void HandleJavaMeta(MetaDataEntry entry)
        {
            base.HandleJavaMeta(entry);
            
            if (entry.Index == 7 && entry is MetadataSlot slot)
            {
                var item = JavaWorldProvider.GetItemFromSlotData(slot.Value);
                if (item != null)
                {
                    SetItem(item);
                }
            }
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