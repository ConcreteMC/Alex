using System;
using System.Collections.Generic;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Common.Utils.Collections;
using Alex.Graphics.Models.Entity;
using Microsoft.Xna.Framework;
using NLog;

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

		public Vector3 BaseRotation
		{
			get => _baseRotation;
			set
			{
				_baseRotation = value;
				UpdateTransform();
			}
		}

		public Vector3 BasePosition
		{
			get => _basePosition;
			set
			{
				_basePosition = value;
				UpdateTransform();
			}
		}

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
				_startRotation = _targetRotation = _rotation = value;
				UpdateTransform();
			}
		}

		public Vector3 Position
		{
			get => _position;
			set
			{
				_startPosition = _targetPosition = _position = value;
				UpdateTransform();
			}
		}

		public Vector3 Scale
		{
			get => _scale;
			set
			{
				_targetScale = _scale = value;
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

		//	public bool Rendered { get; set; } = true;

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

		public void MoveOverTime(Vector3 targetPosition, Vector3 targetRotation, TimeSpan time)
		{
			MoveOverTime(targetPosition, targetRotation, _targetScale, time);
		}

		public void MoveOverTime(Vector3 targetPosition, TimeSpan time)
		{
			MoveOverTime(targetPosition, _targetRotation, _targetScale, time);
		}
		
		public void MoveOverTime(Vector3 targetPosition,
			Vector3 targetRotation,
			Vector3 targetScale,
			TimeSpan time,
			bool overrideOthers = false,
			float blendWeight = 1f)
		{
			if (overrideOthers)
			{
				//_startPosition = _position;
				_tempTargetPosition = targetPosition; // - _tempTargetPosition;

				//_startRotation = _rotation;
				_tempTargetRotation = targetRotation; // - _targetRotation;

				//_startScale = _scale;
				_tempTargetScale = targetScale; // - _targetScale;
			}
			else
			{
				//_startPosition = _position;
				_tempTargetPosition += FixInvalidVector(targetPosition); // - _tempTargetPosition;

				//_startRotation = _rotation;
				_tempTargetRotation += FixInvalidVector(targetRotation); // - _targetRotation;

				//_startScale = _scale;
				_tempTargetScale = FixInvalidVector(targetScale); // - _targetScale;
			}

			_tempTarget = time.TotalSeconds;
		}
		
		private Vector3 FixInvalidVector(Vector3 vector)
		{
			vector.X = float.IsNaN(vector.X) ? 0f : vector.X;
			vector.Y = float.IsNaN(vector.Y) ? 0f : vector.Y;
			vector.Z = float.IsNaN(vector.Z) ? 0f : vector.Z;

			return vector;
		}
		
		
		private Vector3 _startRotation = Vector3.Zero;
		private Vector3 _targetRotation = Vector3.Zero;

		private Vector3 _tempTargetRotation = Vector3.Zero;

		private Vector3 _startPosition = Vector3.Zero;
		private Vector3 _targetPosition = Vector3.Zero;

		private Vector3 _tempTargetPosition = Vector3.Zero;

		private Vector3 _startScale = Vector3.One;
		private Vector3 _targetScale = Vector3.One;

		private Vector3 _tempTargetScale = Vector3.One;

		private double _accumulator = 1d;
		private double _target = 1d;

		private double _tempTarget = 1d;
		private Vector3 _scale = Vector3.One;
		private Vector3 _position = Vector3.Zero;

		public void ApplyMovement()
		{
			_startPosition = _position;
			_targetPosition = FixInvalidVector(_tempTargetPosition);
			_tempTargetPosition = Vector3.Zero;

			_startRotation = _rotation;
			_targetRotation = FixInvalidVector(_tempTargetRotation);
			_tempTargetRotation = Vector3.Zero;

			_startScale = _scale;
			_targetScale = FixInvalidVector(_tempTargetScale);
			_tempTargetScale = Vector3.One;

			_accumulator = 0;
			_target = _tempTarget;
		}


		private void UpdateTransform()
		{
			var pivot = Pivot.GetValueOrDefault(new Vector3(8f, 8f, 8f));
			
			Transform = Matrix.CreateScale(_baseScale * _scale) 
			            * Matrix.CreateTranslation(-pivot) 
			            * MatrixHelper.CreateRotationDegrees(_rotation )
			            * MatrixHelper.CreateRotationDegrees(_baseRotation)
			            * Matrix.CreateTranslation(pivot)
			            * Matrix.CreateTranslation(_position + _basePosition);
		}

		public void Update(IUpdateArgs args)
		{
			if (_accumulator < _target)
			{
				_accumulator += args.GameTime.ElapsedGameTime.TotalSeconds;
				float progress = (float) ((1f / _target) * _accumulator);

				var targetRotation = _targetRotation;
				var startRotation = _startRotation;

				var targetPosition = _targetPosition;
				var startPosition = _startPosition;

				var targetScale = _targetScale;
				var startScale = _startScale;

				_rotation = MathUtils.LerpVector3Degrees(startRotation, targetRotation, progress);
				_position = Vector3.Lerp(startPosition, targetPosition, progress);
				_scale =  Vector3.Lerp(startScale, targetScale, progress);

				UpdateTransform();
			}

			foreach (var attached in _attached)
			{
				attached.Update(args);
			}
		}

		private ThreadSafeList<IAttached> _attached = new ThreadSafeList<IAttached>();
		private Vector3 _baseRotation = Vector3.Zero;
		private Vector3 _basePosition = Vector3.Zero;
		private Vector3 _baseScale = Vector3.One;

		/// <inheritdoc />
		public void AddChild(IAttached attachment)
		{
			return;
			if (_attached.Contains(attachment)) return;
			var model = attachment.Model;

			if (model == null)
			{
				Log.Warn($"Failed to add attachment, no model found.");
				return;
			}
				
			attachment.Parent = this;
				
			
			Model.Bones.Add(model.Root);

			foreach (var mesh in model.Meshes)
			{
				Model.Meshes.Add(mesh);
			}
			AddChild(model.Root);
			_attached.Add(attachment);
			//Model.BuildHierarchy();
		}

		/// <inheritdoc />
		public void Remove(IAttached attachment)
		{
			return;
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

			foreach (var mesh in model.Meshes)
			{
				Model.Meshes.Remove(mesh);
			}
				
			Model.Bones.Remove(model.Root);
			RemoveChild(model.Root);
			//Model.BuildHierarchy();
		}
	}
}