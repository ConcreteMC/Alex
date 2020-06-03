using System;
using Alex.API.Graphics;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Graphics.Models.Items;
using Alex.Items;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Elements.Inventory
{
    public class GuiItem : GuiContext3DElement, IGuiContext3DDrawable
    {
        private IItemRenderer _itemRenderer;

        private Item _item;

        public Item Item
        {
            get => _item;
            set
            {
                _item = value?.Clone();
                _itemRenderer = _item?.Renderer?.Clone();
                
                if(_itemRenderer != null)
                    _itemRenderer.DisplayPosition = DisplayPosition.Gui;
                
                Drawable = _itemRenderer == null ? null : this;
            }
        }

        public GuiItem()
        {
            Camera = new ItemViewCamera();
            TargetPosition = new PlayerLocation(Vector3.Zero);
        }

        public void UpdateContext3D(IUpdateArgs args, IGuiRenderer guiRenderer)
        {
            // if (args.Camera is ItemViewCamera itemViewCamera)
            // {
            //     itemViewCamera.Scale = guiRenderer.ScaledResolution.ScaleFactor;
            // }

            var minSize = Math.Min(InnerBounds.Width, InnerBounds.Height);

            Camera.MoveTo(new Vector3(0f, 0f, 2f), new Vector3(0f, 0f, 0f));

            _itemRenderer.Update(Matrix.Identity, 
                new PlayerLocation(Vector3.Zero));
            //_itemRenderer.Update(Matrix.Identity);
            _itemRenderer.Update(args.GraphicsDevice, args.Camera);
        }

        public void DrawContext3D(IRenderArgs args, IGuiRenderer guiRenderer)
        {
            _itemRenderer.Render(args);
        }


        class ItemViewCamera : GuiContext3DCamera
        {
            public ItemViewCamera() : base(new Vector3(0f, 0f, 2f))
            {
                Rotation = Vector3.Zero;
                Target = Vector3.Zero;
                Direction = Vector3.Backward;
            }

            protected override void UpdateViewMatrix()
            {
                Target = Vector3.Zero;
                Direction = Vector3.Forward;
                
                ViewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.Up);
                //ViewMatrix = Matrix.CreateLookAt(Position + CameraPositionOffset, Target, Vector3.Up);
            }

            public override void UpdateProjectionMatrix()
            {
                //ProjectionMatrix = Matrix.CreateOrthographic(2f, 2f, float.Epsilon, 16f);
                //ProjectionMatrix = Matrix.CreateOrthographic(1.5f, 1.5f, NearDistance, FarDistance);
                ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0f, 1f, 0f, 1f, NearDistance, FarDistance);
            }
        }
    }
}