using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ResourcePackLib.ModelExplorer.Graphics
{
    public class DebugGrid
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
        private VertexBuffer          _vertexBuffer;
        private short[]               _indicies;
        private bool                  _dirty;
        private Vector3               _yAxis;
        private Vector3               _xAxis;
        private Color                 _lineColor = Color.LightGray;
        private float                 _spacing   = 1.0f;
        private int                   _steps     = 10;

        private BasicEffect _effect;
        public void Init(GraphicsDevice graphics)
        {
            if (_effect == null)
                _effect = new BasicEffect(graphics)
                {
                    VertexColorEnabled = true,
                    World = Matrix.Identity
                };
            
            var lineCount = (Steps * 2);
            
            var halfSteps = (int)Math.Ceiling(Steps / 2f);

            var arraySize = (short)((((halfSteps * 2f) + 1f) * 2f) * 2);
            _verticies = new VertexPositionColor[arraySize];
            _vertexBuffer = new VertexBuffer(graphics, VertexPositionColor.VertexDeclaration, arraySize, BufferUsage.WriteOnly);
            _indicies = new short[arraySize];
        }

        public void CalculateVerticies()
        {
            var halfSteps = (int)Math.Ceiling(Steps / 2f);

            var arraySize = (short)((((halfSteps * 2f) + 1f) * 2f) * 2);
            if (arraySize != _vertexBuffer.VertexCount) 
                Init(ModelExplorerGame.Instance.Game.GraphicsDevice);
            
            var verticies = new VertexPositionColor[arraySize];
            var indicies = new short[arraySize];

            var minX = -halfSteps * XAxis * Spacing;
            var maxX = halfSteps * XAxis * Spacing;
            
            var minY = -halfSteps * YAxis * Spacing;
            var maxY = halfSteps * YAxis * Spacing;

            int i = 0;
            for (int x = -halfSteps; x <= halfSteps; x++)
            {
                var minPos = (x * XAxis * Spacing) + minY;
                var maxPos = (x * XAxis * Spacing) + maxY;
                verticies[i] = new VertexPositionColor(minPos, LineColor);
                indicies[i] = (short)i;
                i++;
                verticies[i] = new VertexPositionColor(maxPos, LineColor);
                indicies[i] = (short)i;
                i++;
            }
            
            for (int y = -halfSteps; y <= halfSteps; y++)
            {
                var minPos = (y * YAxis * Spacing) + minX;
                var maxPos = (y * YAxis * Spacing) + maxX;
                verticies[i] = new VertexPositionColor(minPos, LineColor);
                indicies[i] = (short)i;
                i++;
                verticies[i] = new VertexPositionColor(maxPos, LineColor);
                indicies[i] = (short)i;
                i++;
            }
            
            _vertexBuffer.SetData(verticies);
            _indicies = indicies;
        }

        public void Update()
        {
            if (_dirty)
            {
                _dirty = false;
                CalculateVerticies();
            }
        }
        
        public void Draw(GraphicsDevice graphics, Matrix view, Matrix projection)
        {
            _effect.View = view;
            _effect.Projection = projection;
            
            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.SetVertexBuffer(_vertexBuffer);
                graphics.DrawUserPrimitives(PrimitiveType.LineList, _verticies, 0, _verticies.Length / 2);
            }
        }
        
    }
}