using System;
using System.Collections.Generic;
using System.Numerics;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Common.Utils.Collections;
using Alex.Graphics.Models.Entity;
using Microsoft.Xna.Framework;
using NLog;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Alex.Graphics.Models
{
	public class ModelBone
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ModelBone));
		
		private List<ModelMesh> _meshes = new List<ModelMesh>();
		public List<ModelMesh> Meshes
		{
			get
			{
				return this._meshes;
			}
			private set
			{
				_meshes = value;
			}
		}
		
		/// <summary>
		///		 Gets a collection of bones that are children of this bone.
		/// </summary>
		public ModelBoneCollection Children { get; private set; }

		/// <summary>
		///		Gets the index of this bone in the Bones collection.
		/// </summary>
		public int Index { get; set; } = -1;
		
		/// <summary>
		///  Gets the name of this bone.
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		///		Gets the parent of this bone.
		/// </summary>
		public ModelBone Parent { get; set; }
		
		/// <summary>
		///		The root model
		/// </summary>
		public Model Model { get; set; }
		

		/// <summary>
		///		Gets or sets the matrix used to transform this bone relative to its parent bone.
		/// </summary>
		public Matrix Transform { get; set; } = Matrix.Identity;
		
		/// <summary>
		/// Transform of this node from the root of the model not from the parent
		/// </summary>
		public Matrix ModelTransform {
			get;
			set;
		} = Matrix.Identity;
		
		private Vector3 _baseRotation = Vector3.Zero;
		public Vector3 BaseRotation
		{
			get => _baseRotation;
			set
			{
				_baseRotation = value;
				UpdateTransform();
			}
		}

		private Vector3 _basePosition = Vector3.Zero;
		public Vector3 BasePosition
		{
			get => _basePosition;
			set
			{
				_basePosition = value;
				UpdateTransform();
			}
		}
		
		private Vector3 _baseScale = Vector3.One;
		public Vector3 BaseScale
		{
			get => _baseScale;
			set
			{
				_baseScale = value;
				UpdateTransform();
			}
		}

		private Vector3 _rotation;
		public Vector3 Rotation
		{
			get => _rotation;
			set
			{
				_rotationData.Start = _rotationData.Target = _rotation = value;
				UpdateTransform();
			}
		}

		private Vector3 _position = Vector3.Zero;
		public Vector3 Position
		{
			get => _position;
			set
			{
				_positionData.Start = _positionData.Target = _position = value;
				UpdateTransform();
			}
		}
		
		private Vector3 _scale = Vector3.One;
		public Vector3 Scale
		{
			get => _scale;
			set
			{
				_scaleData.Start = _scaleData.Target = _scale = value;
				UpdateTransform();
			}
		}

		public Vector3? Pivot
		{
			get => _pivot;
			set
			{
				_pivot = value;
				UpdateTransform();
			}
		}

		public bool Visible { get; set; } = true;
		public ModelBone ()	
		{
			Children = new ModelBoneCollection(new List<ModelBone>());
		}
		public BoundingBox Box { get; set; }
		public void AddMesh(ModelMesh mesh)
		{
			mesh.ParentBone = this;
			_meshes.Add(mesh);
		}

		public void AddChild(ModelBone modelBone)
		{
			modelBone.Parent = this;
			Children.Add(modelBone);
		}

		public void RemoveChild(ModelBone modelBone)
		{
			if (modelBone.Parent != this)
				return;
			
			modelBone.Parent = null;
			Children.Remove(modelBone);
		}
		
		private Vector3? _pivot = null;

		public void RotateOverTime(Vector3 targetRotation, double time, bool overrideOthers = false)
		{
			if (overrideOthers)
			{
				_tempRotationData.Target = targetRotation;
			}
			else
			{
				_tempRotationData.Target += targetRotation;// Quaternion.Multiply(_tempRotationData.Target, rot);
			}

			_tempRotationData.TargetTime = time;
		}
		
		public void TranslateOverTime(Vector3 targetTranslation, double time, bool overrideOthers = false)
		{
			if (overrideOthers)
			{
				_tempPositionData.Target = targetTranslation;
			}
			else
			{
				_tempPositionData.Target += targetTranslation;
			}
			
			_tempPositionData.TargetTime = time;
		}
		
		public void ScaleOverTime(Vector3 targetScale, double time, bool overrideOthers = false)
		{
			if (overrideOthers)
			{
				_tempScaleData.Target = targetScale;
			}
			else
			{
				_tempScaleData.Target += targetScale;// (_tempScaleData.Target + targetScale) / 2f;
			}
			
			_tempScaleData.TargetTime = time;
		}

		
		private InterpolationData<Vector3> _rotationData = RotatedInterpolationData.EmptyRotation;
		private InterpolationData<Vector3> _tempRotationData = RotatedInterpolationData.EmptyRotation;

		private InterpolationData<Vector3> _tempPositionData = VectorInterpolationData.Empty;
		private InterpolationData<Vector3> _positionData = VectorInterpolationData.Empty;

		private InterpolationData<Vector3> _tempScaleData = VectorInterpolationData.Empty;
		private InterpolationData<Vector3> _scaleData = VectorInterpolationData.Empty;

		public void ApplyMovement()
		{
			var posData = _tempPositionData;
			_positionData = _positionData.WithValues(_position, posData.Target, posData.TargetTime);
			_tempPositionData.Reset();

			var rotData = _tempRotationData;
			_rotationData = _rotationData.WithValues(_rotation, rotData.Target, rotData.TargetTime);
			_tempRotationData.Reset();
			
			var scaleData = _tempScaleData;
			_scaleData = _scaleData.WithValues(_scale, scaleData.Target, scaleData.TargetTime);
			_tempScaleData.Reset();
		}


		private void UpdateTransform()
		{
			var box = Box.GetDimensions();
			var pivot = Pivot.GetValueOrDefault(box/ 2f);

			var scale = _scale;
			Transform = Matrix.CreateScale(_baseScale * scale) * Matrix.CreateTranslation(-pivot)
			                                                    * MatrixHelper.CreateRotationDegrees(_baseRotation * new Vector3(1f, 1f, 1f))
			                                                    * MatrixHelper.CreateRotationDegrees((_rotation) * new Vector3(-1f, -1f, 1f))
			                                                    * Matrix.CreateTranslation(pivot)
			                                                    * Matrix.CreateTranslation(_basePosition + (_position * new Vector3(-1f, 1f, 1f)));
		}

		public void Update(IUpdateArgs args)
		{
			var dt = Alex.DeltaTime;
			var rotation = _rotationData;
			var position = _positionData;
			var scale = _scaleData;

			if (rotation.IsValid)
				_rotation = rotation.Update(dt);
			
			if (position.IsValid)
				_position = position.Update(dt);

			if (scale.IsValid)
			{
				var s = scale.Update(dt);

				if (s.X < 0)
					s.X = 0;
				
				if (s.Y < 0)
					s.Y = 0;
				
				if (s.Z < 0)
					s.Z = 0;

				_scale = s;
			}

			UpdateTransform();
		}
	}
	
	public abstract class InterpolationData<V>
	{
		public static InterpolationData<Vector3> Empty => new VectorInterpolationData(Vector3.Zero, Vector3.Zero, -1d);
		public static InterpolationData<Vector3> EmptyRotation => new RotatedInterpolationData(Vector3.Zero, Vector3.Zero, -1d);
		public InterpolationData(V start, V target, double targetTime)
		{
			TargetTime = targetTime;
			Start = start;
			Target = target;
			ElapsedTime = 0d;
		}

		public bool IsValid => TargetTime > 0d && ElapsedTime < TargetTime;

		public V Start;
		public V Target;
		public double TargetTime;
		protected double ElapsedTime;

		public abstract InterpolationData<V> WithValues(V start, V target, double targetTime);
		
		protected abstract V OnUpdate(double elapsedTime);

		public abstract void Reset();
		
		public V Update(double deltaTime)
		{
			if (TargetTime <= 0)
				return Start;

			if (ElapsedTime >= TargetTime)
				return Target;
			
			ElapsedTime += deltaTime;

			return OnUpdate(ElapsedTime);
		}
	}

	public class VectorInterpolationData : InterpolationData<Vector3>
	{
		/// <inheritdoc />
		public VectorInterpolationData(Vector3 start, Vector3 target, double targetTime) : base(
			FixInvalidVector(start), FixInvalidVector(target), targetTime)
		{
			
		}
		
		private static Vector3 FixInvalidVector(Vector3 vector)
		{
			vector.X = float.IsNaN(vector.X) ? 0f : vector.X;
			vector.Y = float.IsNaN(vector.Y) ? 0f : vector.Y;
			vector.Z = float.IsNaN(vector.Z) ? 0f : vector.Z;

			return vector;
		}

		public override InterpolationData<Vector3> WithValues(Vector3 start, Vector3 target, double targetTime)
		{
			Start = FixInvalidVector(start);
			Target = FixInvalidVector(target);
			TargetTime = targetTime;
			ElapsedTime = 0d;
			return this;
		}

		/// <inheritdoc />
		protected override Vector3 OnUpdate(double elapsedTime)
		{
			return MathUtils.LerpVector3Safe(Start, Target, (float) (elapsedTime / TargetTime));
		}

		/// <inheritdoc />
		public override void Reset()
		{
			Start = Vector3.Zero;
			Target = Vector3.Zero;
			TargetTime = -1d;
			ElapsedTime = 0d;
		}
	}

	public class RotatedInterpolationData : InterpolationData<Vector3>
	{
		/// <inheritdoc />
		public RotatedInterpolationData(Vector3 start, Vector3 target, double targetTime) : base(
			start, target, targetTime)
		{
			
		}

		/// <inheritdoc />
		public override InterpolationData<Vector3> WithValues(Vector3 start, Vector3 target, double targetTime)
		{
			Start = start;
			Target = target;
			TargetTime = targetTime;
			ElapsedTime = 0d;
			return this;
		}

		/// <inheritdoc />
		protected override Vector3 OnUpdate(double elapsedTime)
		{
			return MathUtils.LerpVector3Degrees(Start, Target, (float) (elapsedTime / TargetTime));// .Lerp(Start, Target, (float) ((1f / TargetTime) * ElapsedTime));
		}
		
		/// <inheritdoc />
		public override void Reset()
		{
			Start = Vector3.Zero;
			Target = Vector3.Zero;
			TargetTime = -1d;
			ElapsedTime = 0d;
		}
	}
}