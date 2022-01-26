using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ResourcePackLib.ModelExplorer.Abstractions;
using RocketUI;

namespace ResourcePackLib.ModelExplorer.Entities;

public class GridEntity : DrawableEntity
{
    public int Steps
    {
        get => _steps;
        set
        {
            _steps = value;
            _dirty = true;
        }
    }
    public int SubDivisions
    {
        get => _subDivisions;
        set
        {
            _subDivisions = value;
            _dirty = true;
        }
    }
    public float Spacing
    {
        get => _spacing;
        set
        {
            _spacing = value;
            _dirty = true;
        }
    }
    public Color LineColor
    {
        get => _lineColor;
        set
        {
            _lineColor = value;
            _dirty = true;
        }
    }
    public Color SubLineColor
    {
        get => _subLineColor;
        set
        {
            _subLineColor = value;
            _dirty = true;
        }
    }
    public Vector3 XAxis
    {
        get => _xAxis;
        set
        {
            _xAxis = value;
            _dirty = true;
        }
    }
    public Vector3 YAxis
    {
        get => _yAxis;
        set
        {
            _yAxis = value;
            _dirty = true;
        }
    }

    private VertexPositionColor[] _verticies;
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private short[] _indicies;
    private bool _dirty = true;
    private Vector3 _yAxis = Vector3.UnitZ;
    private Vector3 _xAxis = Vector3.UnitX;
    private Color _lineColor = Color.LightGray * 0.85f;
    private Color _subLineColor = Color.LightGray * 0.15f;
    private float _spacing = 1.0f;
    private int _subDivisions = 1;
    private int _steps = 25;

    private BasicEffect _effect;

    public GridEntity(IGame game) : base(game)
    {
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        if (_effect == null)
            _effect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                World = Matrix.Identity
            };

        CalculateVertices();
    }

    private void InitBuffers(short arraySize)
    {
        _verticies = new VertexPositionColor[arraySize];
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, arraySize, BufferUsage.WriteOnly);
        _indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, arraySize, BufferUsage.WriteOnly);
        _indicies = new short[arraySize];
    }

    private void CalculateVertices()
    {
        var subSteps = SubDivisions;
        var steps = Steps * subSteps;
        var spacing = Spacing / (float)subSteps;
        var halfSteps = (int)Math.Ceiling(steps / 2f);
        var vSteps = ((halfSteps * 2f) + 1f) * 2f;
        var arraySize = (short)(vSteps * 2);
        
        if (_vertexBuffer == null || arraySize != _vertexBuffer.VertexCount)
            InitBuffers(arraySize);

        var minX = -halfSteps * XAxis * spacing;
        var maxX = halfSteps * XAxis * spacing;

        var minY = -halfSteps * YAxis * spacing;
        var maxY = halfSteps * YAxis * spacing;

        int i = 0;
        for (int x = -halfSteps; x <= halfSteps; x++)
        {
            var lineColor = ((x % subSteps) == 0) ? LineColor : SubLineColor;
            var minPos = (x * XAxis * spacing) + minY;
            var maxPos = (x * XAxis * spacing) + maxY;
            _verticies[i] = new VertexPositionColor(minPos, lineColor);
            _indicies[i] = (short)i;
            i++;
            _verticies[i] = new VertexPositionColor(maxPos, lineColor);
            _indicies[i] = (short)i;
            i++;
        }

        for (int y = -halfSteps; y <= halfSteps; y++)
        {
            var lineColor = ((y % subSteps) == 0) ? LineColor : SubLineColor;
            var minPos = (y * YAxis * spacing) + minX;
            var maxPos = (y * YAxis * spacing) + maxX;
            _verticies[i] = new VertexPositionColor(minPos, lineColor);
            _indicies[i] = (short)i;
            i++;
            _verticies[i] = new VertexPositionColor(maxPos, lineColor);
            _indicies[i] = (short)i;
            i++;
        }

        _vertexBuffer.SetData(_verticies);
        _indexBuffer.SetData(_indicies);
    }
    public override void Update(GameTime gameTime)
    {
        if (_dirty)
        {
            _dirty = false;
            CalculateVertices();
        }
    }
        
    public override void Draw(GameTime gameTime)
    {
        using (GraphicsContext.CreateContext(GraphicsDevice, BlendState.AlphaBlend, DepthStencilState.Default, RasterizerState.CullNone))
        {
            var camera = ((IGame)Game).Camera;
            _effect.View = camera.View;
            _effect.Projection = camera.Projection;
            
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.Indices = _indexBuffer;

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, _indicies.Length / 2);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
    }
}