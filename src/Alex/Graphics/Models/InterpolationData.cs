using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models;

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