using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics
{
    public class DebugAxisComponent : DrawableGameComponent
    {
        public BasicEffect Effect { get; private set; }
        VertexPositionColor[] lines;
        
        public DebugAxisComponent(Game game) : base(game)
        {
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            Effect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                Projection = Matrix.Identity,
                View = Matrix.Identity,
                World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up)
            };
            
            SetLineVerticles();
        }
        protected void SetLineVerticles()
        {

            lines = new VertexPositionColor[6];

            lines[0] = new VertexPositionColor(new Vector3(-1, 0, 0), Color.Red);

            lines[1] = new VertexPositionColor(new Vector3(10,0,0), Color.Red);

            lines[2] = new VertexPositionColor(new Vector3(0, -1, 0), Color.Green);

            lines[3] = new VertexPositionColor(new Vector3(0,10,0), Color.Green);

            lines[4] = new VertexPositionColor(new Vector3(0, 0, -1), Color.Blue);

            lines[5] = new VertexPositionColor(new Vector3(0,0,10), Color.Blue);
        } 

        public override void Draw(GameTime gameTime)
        {

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass?.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, lines, 0, 3);
            }
            
            
            base.Draw(gameTime);
        }
    }
}