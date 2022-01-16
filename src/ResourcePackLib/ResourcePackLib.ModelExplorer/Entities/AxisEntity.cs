using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ResourcePackLib.ModelExplorer.Abstractions;
using RocketUI;

namespace ResourcePackLib.ModelExplorer.Entities;

public class AxisEntity : DrawableGameComponent
{
    public AxisEntity(IGame game) : base(game.Game)
    {
        DrawOrder = 1;
    }

    private const float Distance = 10000;
    private VertexPositionColor[] Verticies;

    protected override void LoadContent()
    {
        base.LoadContent();
        Verticies = new VertexPositionColor[6];

        var i = 0;
        Verticies[i++] = new VertexPositionColor(-Vector3.UnitX * Distance, Color.Red);
        Verticies[i++] = new VertexPositionColor(Vector3.UnitX * Distance, Color.Red);

        Verticies[i++] = new VertexPositionColor(-Vector3.UnitY * Distance, Color.Green);
        Verticies[i++] = new VertexPositionColor(Vector3.UnitY * Distance, Color.Green);

        Verticies[i++] = new VertexPositionColor(-Vector3.UnitZ * Distance, Color.Blue);
        Verticies[i++] = new VertexPositionColor(Vector3.UnitZ * Distance, Color.Blue);
    }

    public override void Draw(GameTime gameTime)
    {
        using (GraphicsContext.CreateContext(GraphicsDevice, BlendState.AlphaBlend, DepthStencilState.Default, RasterizerState.CullNone))
        {
            GraphicsDevice.VertexSamplerStates[0] = SamplerState.PointWrap;
            Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, Verticies, 0, 3);
        }
    }
}