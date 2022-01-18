using Alex.Common.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models;

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