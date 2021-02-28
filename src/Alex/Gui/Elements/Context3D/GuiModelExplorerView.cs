using RocketUI;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Gamestates;
using Alex.Gamestates.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Elements.Context3D
{
    public class GuiModelExplorerView : GuiContext3DElement
    {
        //private PlayerLocation _entityLocation;

     //
     /*public PlayerLocation EntityPosition
        {
            get { return _entityLocation; }
            set
            {
                _entityLocation = value;
                ModelExplorer?.SetLocation(_entityLocation);
            }
        }*/

     public ModelExplorer ModelExplorer
        {
            get => _modelExplorer;
            set
            {
                _modelExplorer = value;
                Drawable = value;
           //     EntityPosition = new PlayerLocation(Vector3.Zero);
            }
        }

       // private GuiContext3DElement.GuiContext3DCamera Camera { get; }

       public GuiModelExplorerView(ModelExplorer modelExplorer, Vector3 cameraOffset, Vector3 cameraTargetPositionOffset) :
            this(modelExplorer, cameraOffset)
        {
            Camera.TargetPositionOffset = cameraTargetPositionOffset;
        }

        public GuiModelExplorerView(ModelExplorer modelExplorer, Vector3 cameraOffset) : this(modelExplorer)
        {
            Camera.TargetPositionOffset = cameraOffset;
        }

        public GuiModelExplorerView(ModelExplorer modelExplorer)
        {
            ModelExplorer = modelExplorer;
           // EntityPosition = new PlayerLocation(Vector3.Zero);
            Background = GuiTextures.PanelGeneric;

            //Camera = new GuiEntityModelViewCamera(this);
          //  Camera = new GuiContext3DElement.GuiContext3DCamera(EntityPosition);
          Camera.SetRenderDistance(128);
         // Camera.CameraPositionOffset = new Vector3(0f, 0f, -6f);
         // Camera.TargetPositionOffset = new Vector3(0f, 1.8f, 0f);
        }

        public void SetEntityRotation(float yaw, float pitch)
        {
          //  EntityPosition.Yaw = yaw;
         //   EntityPosition.Pitch = pitch;
            //TODO: Check what is correct.
            //ViewMatrix = Matrix.CreateLookAt(Target + lookAtOffset, Target + (Vector3.Up * Player.EyeLevel), Vector3.Up);
        }

        public void SetEntityRotation(float yaw, float pitch, float headYaw)
        {
          //  EntityPosition.Yaw = yaw;
          //  EntityPosition.Pitch = pitch;
           // EntityPosition.HeadYaw = headYaw;
        }

        private void InitRenderer()
        {
            /*  if (string.IsNullOrWhiteSpace(EntityName) || SkinTexture == null)
              {
                  _canRender = false;
                  EntityModelRenderer = null;
                  return;
              }
              Alex.Instance.Resources.BedrockResourcePack.EntityModels.TryGetValue(EntityName, out EntityModel m);

              EntityModelRenderer = new EntityModelRenderer(m, SkinTexture);
              _canRender = true;*/
        }

       // private Rectangle _previousBounds;
        private ModelExplorer _modelExplorer;
    }
}