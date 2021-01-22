using Alex.Api;
using Alex.API.Graphics;
using Alex.API.Utils;
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
    public class ItemEntity : ItemBaseEntity
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        public ItemEntity(World level) : base(level)
        {
            Width = 0.25;
            Height = 0.25;
          //  Length = 0.25;
            
            Gravity = 0.04;
            Drag = 0.02;
        }

        private float _rotation = 0;
        protected bool DoRotation { get; set; } = true;
        public override void Update(IUpdateArgs args)
        {
            if (CanRender)
            {
                var knownPos = KnownPosition.ToVector3();
            //    var bb       = base.BoundingBox;
               // var knownPos = bb.GetCenter();
                
                if (DoRotation)
                {
                    //var offset = new Vector3((float) Width, (float) Height, (float) Width) / 2f;
                    var offset = new Vector3(0.5f, 0f, 0.5f);
                    ItemRenderer.Update(args, MCMatrix.CreateScale(0.25f)
                                              * MCMatrix.CreateTranslation(-offset)
                                              * MCMatrix.CreateRotationY(MathHelper.ToRadians(_rotation)) 
                                              * MCMatrix.CreateTranslation(offset)
                                              * MCMatrix.CreateTranslation(knownPos));
                }
                else
                {
                    ItemRenderer.Update(args,  MCMatrix.CreateScale(0.25f)
                                               * MCMatrix.CreateRotationY(MathHelper.ToRadians(KnownPosition.Yaw))
                                               * MCMatrix.CreateTranslation(knownPos));
                }
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
            
            ItemRenderer?.Render(renderArgs, null);
        }
    }
}