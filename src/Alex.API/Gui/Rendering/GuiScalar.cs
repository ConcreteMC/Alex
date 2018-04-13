namespace Alex.API.Gui.Rendering
{
    public class GuiScalar
    {

        public float Scale { get; set; }

        public int Offset { get; set; }


        public GuiScalar(float scale, int offset)
        {
            Scale = scale;
            Offset = offset;
        }


        public int ToAbsolute(int? parentValue)
        {
            return (int)(parentValue.HasValue ? (parentValue * Scale) : 0) + Offset;
        }

        public static GuiScalar FromAbsolute(int value)
        {
            return new GuiScalar(0f, value);
        }

        public static GuiScalar FromRelative(float scale, int offset = 0)
        {
            return new GuiScalar(scale, offset);
        }
    }
}
