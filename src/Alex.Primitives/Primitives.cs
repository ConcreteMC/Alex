namespace Alex.Interfaces
{
	public static class Primitives
	{
		public static IPrimitiveFactory Factory { get; set; }

		public static class Color
		{
			public static IColor White => Primitives.Factory.Color(255, 255, 255);
			public static IColor Black => Primitives.Factory.Color(0, 0, 0);
		}
	}
}