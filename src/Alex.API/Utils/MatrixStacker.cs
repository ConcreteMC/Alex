using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace Alex.API.Utils
{

    public enum MatrixMode
    {
        Projection,
        ModelView
    }

    public class MatrixStacker
    {
        class MatrixState
        {
            public Matrix Matrix { get; set; }
        }

        public event EventHandler<Matrix> OnProjectionChanged;
        public event EventHandler<Matrix> OnModelViewChanged;

        private readonly Stack<MatrixState> _projectionStack = new Stack<MatrixState>();
        private readonly Stack<MatrixState> _modelViewStack = new Stack<MatrixState>();
        
        private MatrixMode _mode;

        public MatrixMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
            }
        }

        private MatrixState _projection = new MatrixState();

        public Matrix Projection
        {
            get => _projection.Matrix;
            private set
            {
                _projection.Matrix = value;

                OnProjectionChanged?.Invoke(this, _projection.Matrix);
            }
        }

        private MatrixState _modelView = new MatrixState();

        public Matrix ModelView
        {
            get => _modelView.Matrix;
            private set
            {
                _modelView.Matrix = value;
                OnModelViewChanged?.Invoke(this, _modelView.Matrix);
            }
        }

        public Matrix Matrix
        {
            get
            {
                if (Mode == MatrixMode.Projection)
                    return Projection;
                if (Mode == MatrixMode.ModelView)
                    return ModelView;

                throw new InvalidEnumArgumentException(nameof(Mode), (int)Mode, typeof(MatrixMode));
            }
            private set
            {
                if (Mode == MatrixMode.Projection)
                    Projection = value;
                if (Mode == MatrixMode.ModelView)
                    ModelView = value;

                throw new InvalidEnumArgumentException(nameof(Mode), (int)Mode, typeof(MatrixMode));
            }
        }

        private MatrixState State
        {
            get
            {
                if (Mode == MatrixMode.Projection)
                    return _projection;
                if (Mode == MatrixMode.ModelView)
                    return _modelView;

                throw new InvalidEnumArgumentException(nameof(Mode), (int)Mode, typeof(MatrixMode));
            }
            set
            {
                if (Mode == MatrixMode.Projection)
                {
                    _projection = value;
                    OnProjectionChanged?.Invoke(this, _projection.Matrix);
                }

                if (Mode == MatrixMode.ModelView)
                {
                    _modelView = value;
                    OnModelViewChanged?.Invoke(this, _modelView.Matrix);
                }

                throw new InvalidEnumArgumentException(nameof(Mode), (int)Mode, typeof(MatrixMode));
            }
        }

        private Stack<MatrixState> Stack
        {
            get
            {
                if (Mode == MatrixMode.Projection)
                    return _projectionStack;
                if (Mode == MatrixMode.ModelView)
                    return _modelViewStack;

                throw new InvalidEnumArgumentException(nameof(Mode), (int)Mode, typeof(MatrixMode));
            }
        }

        public MatrixStacker()
        {
            Mode = MatrixMode.ModelView;
            Push();
            LoadIdentity();

            Mode = MatrixMode.Projection;
            Push();
            LoadIdentity();
        }

        public void LoadIdentity()
        {
            Matrix = Matrix.Identity;
        }

        public void Push()
        {
            var m = Matrix;
            var s  = new MatrixState() { Matrix = m };

            Stack.Push(s);
            State = s;
        }

        public void Pop()
        {
            if (Stack.Count == 0)
            {
                LoadIdentity();
                return;
            }

            Stack.Pop();

            State = Stack.Peek();
        }

        public void Multiply(Matrix matrix)
        {
            Matrix *= matrix;
        }

        public void Perspective(float fov, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            Matrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), aspectRatio, nearPlaneDistance, farPlaneDistance);
        }

        public void Translate(float x, float y, float z)
        {
            Multiply(Matrix.CreateTranslation(x, y, z));
        }

        public void Translate(Vector3 pos)
        {
            Multiply(Matrix.CreateTranslation(pos));
        }

        public void RotateX(float angle)
        {
            Multiply(Matrix.CreateRotationX(MathHelper.ToRadians(angle)));
        }
        
        public void RotateY(float angle)
        {
            Multiply(Matrix.CreateRotationY(MathHelper.ToRadians(angle)));
        }

        public void RotateZ(float angle)
        {
            Multiply(Matrix.CreateRotationZ(MathHelper.ToRadians(angle)));
        }

    }

}
