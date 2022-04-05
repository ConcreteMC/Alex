using Alex.Interfaces;

namespace Alex.ResourcePackLib.Json.Models
{
	public class DisplayElement
	{
		public IVector3 Rotation { get; set; } = Primitives.Factory.Vector3Zero;
		public IVector3 Translation { get; set; } = Primitives.Factory.Vector3Zero;
		public IVector3 Scale { get; set; } = Primitives.Factory.Vector3Zero;

		public DisplayElement() { }

		public DisplayElement(IVector3 rotation, IVector3 translation, IVector3 scale)
		{
			Rotation = rotation;
			Translation = translation;
			Scale = scale;
		}

		public static readonly DisplayElement Default = new DisplayElement(Primitives.Factory.Vector3Zero, Primitives.Factory.Vector3Zero, Primitives.Factory.Vector3Zero);

		public DisplayElement Clone()
		{
			return new DisplayElement(
				Primitives.Factory.Vector3(Rotation.X, Rotation.Y, Rotation.Z),
				Primitives.Factory.Vector3(Translation.X, Translation.Y, Translation.Z), Primitives.Factory.Vector3(Scale.X, Scale.Y, Scale.Z));
		}
	}
}