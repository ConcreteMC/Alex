
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

        private   float _rotation = 0;
        protected bool  DoRotation { get; set; } = true;
        private   bool  IsBlock    { get; set; } = false;
        public override void Update(IUpdateArgs args)
        {
            if (CanRender)
            {
                var knownPos = KnownPosition.ToVector3();
                // var knownPos = bb.GetCenter();
               float scale = (float) (IsBlock ? (1f / (1f / Width)) : (1f / 32f));
                if (DoRotation)
                {
                    //var offset = new Vector3((float) Width, (float) Height, (float) Width) / 2f;
                    var offset = new Vector3((float) Width, 0f, (float) Width);
                    ItemRenderer.Update(args, Matrix.CreateScale(scale)
                                             // * MCMatrix.CreateTranslation(-offset)
                                              * Matrix.CreateRotationY(MathHelper.ToRadians(_rotation)) 
                                            //  * MCMatrix.CreateTranslation(offset)
                                              * Matrix.CreateTranslation(knownPos), new Vector3(scale));
                }
                else
                {
                    ItemRenderer.Update(args,  Matrix.CreateScale(scale)
                                               * Matrix.CreateRotationY(MathHelper.ToRadians(KnownPosition.Yaw))
                                               * Matrix.CreateTranslation(knownPos), new Vector3(scale));
                }
            }

            if (DoRotation)
            {
                _rotation += 45f * (float) args.GameTime.ElapsedGameTime.TotalSeconds;
            }
        }


        /// <inheritdoc />
        public override void SetItem(Item item)
        {
            base.SetItem(item);

            if (item is ItemBlock)
                IsBlock = true;
        }

        public override void Render(IRenderArgs renderArgs)
        {
            if (!CanRender)
                return;
            
            ItemRenderer?.Render(renderArgs, null);
        }
    }
}