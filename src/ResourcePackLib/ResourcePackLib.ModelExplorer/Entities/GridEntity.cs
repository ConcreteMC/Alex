using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ResourcePackLib.ModelExplorer.Abstractions;

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
    private bool _dirty;
    private Vector3 _yAxis = Vector3.UnitZ;
    private Vector3 _xAxis = Vector3.UnitX;
    private Color _lineColor = Color.LightGray;
    private float _spacing = 1.0f;
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

        var halfSteps = (int)Math.Ceiling(Steps / 2f);

        var arraySize = (short)((((halfSteps * 2f) + 1f) * 2f) * 2);
        InitBuffers(arraySize);
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
        var halfSteps = (int)Math.Ceiling(Steps / 2f);

        var arraySize = (short)((((halfSteps * 2f) + 1f) * 2f) * 2);
        if (arraySize != _vertexBuffer.VertexCount)
            InitBuffers(arraySize);

        var minX = -halfSteps * XAxis * Spacing;
        var maxX = halfSteps * XAxis * Spacing;

        var minY = -halfSteps * YAxis * Spacing;
        var maxY = halfSteps * YAxis * Spacing;

        int i = 0;
        for (int x = -halfSteps; x <= halfSteps; x++)
        {
            var minPos = (x * XAxis * Spacing) + minY;
            var maxPos = (x * XAxis * Spacing) + maxY;
            _verticies[i] = new VertexPositionColor(minPos, LineColor);
            _indicies[i] = (short)i;
            i++;
            _verticies[i] = new VertexPositionColor(maxPos, LineColor);
            _indicies[i] = (short)i;
            i++;
        }

        for (int y = -halfSteps; y <= halfSteps; y++)
        {
            var minPos = (y * YAxis * Spacing) + minX;
            var maxPos = (y * YAxis * Spacing) + maxX;
            _verticies[i] = new VertexPositionColor(minPos, LineColor);
            _indicies[i] = (short)i;
            i++;
            _verticies[i] = new VertexPositionColor(maxPos, LineColor);
            _indicies[i] = (short)i;
            i++;
        }

        _vertexBuffer.SetData(_verticies);
        _indexBuffer.SetData(_indicies);
    }
    
    public void Update()
    {
        if (_dirty)
        {
            _dirty = false;
            CalculateVertices();
        }
    }
        
    public void Draw(GraphicsDevice graphics, Matrix view, Matrix projection)
    {
        _effect.View = view;
        _effect.Projection = projection;
            
        graphics.SetVertexBuffer(_vertexBuffer);
        graphics.Indices = _indexBuffer;
        
        foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphics.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, _indicies.Length / 2);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
    }
}