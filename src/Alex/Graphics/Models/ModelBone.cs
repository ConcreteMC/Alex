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
	public class ModelBone : IHoldAttachment
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ModelBone));
		
		private List<ModelBone> _children = new List<ModelBone>();
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
			//if (overrideOthers)
			//{
				_tempScaleData.Target = targetScale;
			//}
			//else
			{
			//	_tempScaleData.Target += targetScale;
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
			_tempPositionData = VectorInterpolationData.Empty;
			_positionData = posData.WithStart(_position);

			var rotData = _tempRotationData;
			_tempRotationData = RotatedInterpolationData.EmptyRotation;
			_rotationData = rotData.WithStart(_rotation);


			var scaleData = _tempScaleData;
			_tempScaleData = VectorInterpolationData.Empty;
			_scaleData = scaleData.WithStart(_scale);
		}


		private void UpdateTransform()
		{
			var pivot = Pivot.GetValueOrDefault(new Vector3(8f, 8f, 8f));

			// * MatrixHelper.CreateRotation(Quaternion.Multiply(_rotation, _baseRotation).ToEuler())
			Transform = Matrix.CreateScale(_baseScale * _scale) * Matrix.CreateTranslation(-pivot)
			                                                    * MatrixHelper.CreateRotationDegrees(_baseRotation)
			                                                    * MatrixHelper.CreateRotationDegrees(_rotation * new Vector3(-1f, 1f, 1f))
			                                                    //* MatrixHelper.CreateRotation(Quaternion.Multiply(_rotation, _baseRotation).ToEuler())
			                                                    // * MatrixHelper.CreateRotationDegrees(_baseRotation)
			                                                    * Matrix.CreateTranslation(pivot)
			                                                    * Matrix.CreateTranslation(_position + _basePosition);
		}

		public void Update(IUpdateArgs args)
		{
			var dt = args.GameTime.ElapsedGameTime.TotalSeconds;
			var rotation = _rotationData;
			var position = _positionData;
			var scale = _scaleData;

			if (rotation.IsValid)
				_rotation = rotation.Update(dt);
			
			if (position.IsValid)
				_position = position.Update(dt);
			
			if (scale.IsValid)
				_scale = scale.Update(dt);

			UpdateTransform();
		}

		private ThreadSafeList<IAttached> _attached = new ThreadSafeList<IAttached>();
		/// <inheritdoc />
		public void AddChild(IAttached attachment)
		{
			if (_attached.Contains(attachment)) return;
			var model = attachment.Model;

			if (model == null)
			{
				Log.Warn($"Failed to add attachment, no model found.");
				return;
			}
				
			attachment.Parent = this;
			
			foreach (var mesh in model.Meshes)
			{
				Model.Meshes.Add(mesh);
			}
			AddChild(model.Root);
			Model.Bones.Add(model.Root);
			_attached.Add(attachment);
			//Model.BuildHierarchy();
		}

		/// <inheritdoc />
		public void Remove(IAttached attachment)
		{
			if (!_attached.Remove(attachment)) 
				return;

			if (attachment.Parent != this)
				return;
			
			attachment.Parent = null;
			var model = attachment.Model;

			if (model == null)
			{
				Log.Warn($"Failed to remove attachment, no model found.");
				return;
			}

			Model.Bones.Remove(model.Root);
			foreach (var mesh in model.Meshes)
			{
				Model.Meshes.Remove(mesh);
			}
			RemoveChild(model.Root);
			//Model.BuildHierarchy();
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

		public bool IsValid => TargetTime > 0d;

		public V Start;
		public V Target;
		public double TargetTime;
		protected double ElapsedTime;

		public abstract InterpolationData<V> WithStart(V start);
		protected abstract V OnUpdate(double deltaTime);

		public V Update(double deltaTime)
		{
			if (TargetTime <= 0)
				return Start;

			if (ElapsedTime >= TargetTime)
				return Target;
			
			ElapsedTime += deltaTime;

			return OnUpdate(deltaTime);
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

		/// <inheritdoc />
		public override InterpolationData<Vector3> WithStart(Vector3 start)
		{
			return new VectorInterpolationData(start, Target, TargetTime);
		}

		/// <inheritdoc />
		protected override Vector3 OnUpdate(double deltaTime)
		{
			return Vector3.Lerp(Start, Target, (float) ((1f / TargetTime) * ElapsedTime));
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
		public override InterpolationData<Vector3> WithStart(Vector3 start)
		{
			return new RotatedInterpolationData(start, Target, TargetTime);
		}
		
		/// <inheritdoc />
		protected override Vector3 OnUpdate(double deltaTime)
		{
			return MathUtils.LerpVector3Degrees(Start, Target, (float) ((1f / TargetTime) * ElapsedTime));// .Lerp(Start, Target, (float) ((1f / TargetTime) * ElapsedTime));
		}
	}
}