using Alex.Common.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models;

public class RotatedInterpolationData : InterpolationData<Vector3>
{
	/// <inheritdoc />
	public RotatedInterpolationData(Vector3 start, Vector3 target, double targetTime) : base(
		start, target, targetTime) { }

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
		return MathUtils.LerpVector3Degrees(
			Start, Target,
			(float)(elapsedTime / TargetTime)); // .Lerp(Start, Target, (float) ((1f / TargetTime) * ElapsedTime));
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