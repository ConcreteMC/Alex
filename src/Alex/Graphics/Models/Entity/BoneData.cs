using System;
using System.Collections.Generic;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Graphics.Models.Entity
{
	public class BoneData : IHoldAttachment
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BoneData));
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
				//Log.Info($"Updating rotation for bone. Name={Name} Rotation={value}");
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
				/*_targetScale =*/ _scale = value;
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
				_tempTargetScale += FixInvalidVector(targetScale); // - _targetScale;
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

		private Vector3 _baseRotation = Vector3.Zero;
		private Vector3 _basePosition = Vector3.Zero;
		private Vector3 _baseScale = Vector3.One;
		
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


		public Matrix Transform { get; set; } = Matrix.Identity;
		private void UpdateTransform()
		{
			Matrix matrix = Matrix.Identity;

			var pivot = Pivot.GetValueOrDefault(new Vector3(8f, 8f, 8f));


			matrix = Matrix.CreateTranslation(-pivot) * MatrixHelper.CreateRotationDegrees(_rotation )
			                                          * MatrixHelper.CreateRotationDegrees(_baseRotation)
			                                          * Matrix.CreateTranslation(pivot)
			                                          * Matrix.CreateTranslation(_position + _basePosition);

			Transform = Matrix.CreateScale(_baseScale) * matrix;
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

				//var rotationDifference = targetRotation - startRotation;
				_rotation = startRotation + ((targetRotation - startRotation) * progress);
				_position = startPosition + ((targetPosition - startPosition) * progress);
				_scale = startScale + ((targetScale - startScale) * progress);

				UpdateTransform();
			}

			foreach (var attached in _attached)
			{
				attached.Update(args);
			}
		}
		
		private List<IAttached> _attached = new List<IAttached>();
		
		/// <inheritdoc />
		public void AddChild(IAttached attachment)
		{
			if (!_attached.Contains(attachment))
			{
				var model = attachment.Model;

				if (model == null)
				{
					Log.Warn($"Failed to add attachment, no model found.");
					return;
				}

				//var root = model.Root;
				
			}
		}

		/// <inheritdoc />
		public void Remove(IAttached attachment)
		{
			if (_attached.Remove(attachment))
			{
				attachment.Parent = null;

				var model = attachment.Model;

				if (model == null)
				{
					Log.Warn($"Failed to remove attachment, no model found.");
					return;
				}

				
			}
		}
	}
}