using Alex.API.Graphics;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Elements
{
    public class GuiEntityModelView : GuiContext3DElement
    {
        public override PlayerLocation TargetPosition
        {
            get { return Entity?.KnownPosition ?? new PlayerLocation(Vector3.Zero); }
            set
            {
                if (Entity != null) Entity.KnownPosition = value;
            }
        }

        private Entity _entity;

        public Entity Entity
        {
            get => _entity;
            set
            {
                _entity = value;
                Drawable = _entity?.ModelRenderer == null ? null : new EntityDrawable(_entity);
            }
        }

        public GuiEntityModelView(Entity entity)
        {
            Background = GuiTextures.PanelGeneric;
            Entity = entity;
            Camera.CameraPositionOffset = new Vector3(0f, 0f, -6f);
            Camera.TargetPositionOffset = new Vector3(0f, 1.8f, 0f);
        }

        public void SetEntityRotation(float yaw, float pitch)
        {
            TargetPosition.Yaw = yaw;
            TargetPosition.Pitch = pitch;
        }

        public void SetEntityRotation(float yaw, float pitch, float headYaw)
        {
            TargetPosition.Yaw = yaw;
            TargetPosition.Pitch = pitch;
            TargetPosition.HeadYaw = headYaw;
        }

        class EntityDrawable : IGuiContext3DDrawable
        {
            public Entity Entity { get; }

            public EntityDrawable(Entity entity)
            {
                Entity = entity;
            }

            public void UpdateContext3D(IUpdateArgs args, IGuiRenderer guiRenderer)
            {
                Entity?.Update(args);
            }

            public void DrawContext3D(IRenderArgs args, IGuiRenderer guiRenderer)
            {
                Entity?.Render(args);
            }
        }
    }
}