using Alex.Common.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models;

public class BoneMatrices
{
	private readonly ModelMatrixHolder _parent;

	public BoneMatrices(ModelMatrixHolder parent)
	{
		_parent = parent;
	}

	private Vector3 _rotation = Vector3.Zero;

	public Vector3 Rotation
	{
		get => _rotation;
		set
		{
			_tempRotationData.Reset();
			_rotationData.Reset();
			_rotation = value;
			UpdateTransform();
		}
	}

	private Vector3 _position = Vector3.Zero;

	public Vector3 Position
	{
		get => _position;
		set
		{
			_tempPositionData.Reset();
			_positionData.Reset();
			_position = value;
			UpdateTransform();
		}
	}

	private Vector3 _scale = Vector3.One;

	public Vector3 Scale
	{
		get => _scale;
		set
		{
			_tempScaleData.Reset();
			_scaleData.Reset();
			_scale = value;
			UpdateTransform();
		}
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

		if (posData.IsValid)
		{
			_positionData = _positionData.WithValues(_position, posData.Target, posData.TargetTime);
			_tempPositionData.Reset();
		}

		var rotData = _tempRotationData;

		if (rotData.IsValid)
		{
			_rotationData = _rotationData.WithValues(_rotation, rotData.Target, rotData.TargetTime);
			_tempRotationData.Reset();
		}

		var scaleData = _tempScaleData;

		if (scaleData.IsValid)
		{
			_scaleData = _scaleData.WithValues(_scale, scaleData.Target, scaleData.TargetTime);
			_tempScaleData.Reset();
		}
	}

	private void UpdateTransform()
	{
		_parent.IsMatricesDirty = true;
	}

	public void Update(IUpdateArgs args)
	{
		var dt = Alex.DeltaTime;
		var rotation = _rotationData;
		var position = _positionData;
		var scale = _scaleData;

		bool changed = false;

		if (rotation.IsValid)
		{
			_rotation = rotation.Update(dt);
			changed = true;
		}

		if (position.IsValid)
		{
			_position = position.Update(dt);
			changed = true;
		}

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
			changed = true;
		}

		_parent.IsMatricesDirty = _parent.IsMatricesDirty || changed;
	}

	public void RotateOverTime(Vector3 targetRotation, double time, bool overrideOthers = false)
	{
		if (overrideOthers)
		{
			_tempRotationData.Target = targetRotation;
		}
		else
		{
			_tempRotationData.Target += targetRotation; // Quaternion.Multiply(_tempRotationData.Target, rot);
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
			_tempScaleData.Target = (_tempScaleData.Target + targetScale) / 2f;
		}

		_tempScaleData.TargetTime = time;
	}
}