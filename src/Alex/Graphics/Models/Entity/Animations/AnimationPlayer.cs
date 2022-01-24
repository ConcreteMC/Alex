using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class AnimationPlayer
	{
		private SkinningData _skinningData;
		private Matrix[] _boneTransforms;
		private Matrix[] _worldTransforms;
		private Matrix[] _skinTransforms;

		private AnimationClip _currentClip;
		private TimeSpan _currentTime;
		private int _currentKeyFrame;

		public AnimationPlayer(SkinningData skinningData)
		{
			if (skinningData == null)
				throw new ArgumentNullException(nameof(skinningData));

			_skinningData = skinningData;
			_boneTransforms = new Matrix[skinningData.BindPose.Count];
			_worldTransforms = new Matrix[skinningData.BindPose.Count];
			_skinTransforms = new Matrix[skinningData.BindPose.Count];
		}

		public void StartClip(AnimationClip clip)
		{
			_currentClip = clip;
			_currentTime = TimeSpan.Zero;
			_currentKeyFrame = 0;

			_skinningData.BindPose.CopyTo(_boneTransforms, 0);
		}

		public void Update(TimeSpan time, bool relativeToCurrentTime, Matrix rootTransform)
		{
			UpdateBoneTransforms(time, relativeToCurrentTime);
			UpdateWorldTransforms(rootTransform);
			UpdateSkinTransforms();
		}

		public void UpdateBoneTransforms(TimeSpan time, bool relativeToCurrentTime)
		{
			if (_currentClip == null)
				throw new InvalidOperationException("AnimationPlayer.Update was called before StartClip");

			if (relativeToCurrentTime)
			{
				time += _currentTime;

				while (time >= _currentClip.Duration)
					time -= _currentClip.Duration;
			}

			if (time < TimeSpan.Zero || time >= _currentClip.Duration)
				throw new ArgumentOutOfRangeException(nameof(time));

			if (time < _currentTime)
			{
				_currentKeyFrame = 0;
				_skinningData.BindPose.CopyTo(_boneTransforms, 0);
			}

			_currentTime = time;

			var keyframes = _currentClip.Keyframes;

			while (_currentKeyFrame < keyframes.Count)
			{
				Keyframe keyframe = keyframes[_currentKeyFrame];

				if (keyframe.Time > _currentTime)
					break;

				_boneTransforms[keyframe.Bone] = keyframe.Transform;
				_currentKeyFrame++;
			}
		}

		public void UpdateWorldTransforms(Matrix rootTransforms)
		{
			_worldTransforms[0] = _boneTransforms[0] * rootTransforms;

			for (int bone = 1; bone < _worldTransforms.Length; bone++)
			{
				int parentBone = _skinningData.SkeletonHierarchy[bone];
				_worldTransforms[bone] = _boneTransforms[bone] * _worldTransforms[parentBone];
			}
		}

		public void UpdateSkinTransforms()
		{
			for (int bone = 0; bone < _skinTransforms.Length; bone++)
			{
				_skinTransforms[bone] = _skinningData.InverseBindPose[bone] * _worldTransforms[bone];
			}
		}

		/// Gets the current bone transform matrices, relative to their parent bones.
		/// </summary>
		public Matrix[] GetBoneTransforms()
		{
			return _boneTransforms;
		}


		/// <summary>
		/// Gets the current bone transform matrices, in absolute format.
		/// </summary>
		public Matrix[] GetWorldTransforms()
		{
			return _worldTransforms;
		}


		/// <summary>
		/// Gets the current bone transform matrices,
		/// relative to the skinning bind pose.
		/// </summary>
		public Matrix[] GetSkinTransforms()
		{
			return _skinTransforms;
		}
	}
}