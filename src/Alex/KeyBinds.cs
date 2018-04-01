using Microsoft.Xna.Framework.Input;

namespace Alex
{
    public static class KeyBinds
    {
        public static Keys Forward = Keys.W;
        public static Keys Backward = Keys.S;
        public static Keys Left = Keys.A;
        public static Keys Right = Keys.D;
        public static Keys Up = Keys.Space;
        public static Keys Down = Keys.LeftShift;

	    public static Keys IncreaseSpeed = Keys.OemPlus;
	    public static Keys DecreaseSpeed = Keys.OemMinus;
	    public static Keys ResetSpeed = Keys.OemPeriod;

		public static Keys Fog = Keys.F;

	    public static Keys Menu = Keys.Escape;
	    public static Keys DebugInfo = Keys.F3;

        public static Keys Chat = Keys.T;

	    public static Keys ChangeCamera = Keys.F5;
        public static Keys ToggleFreeCam = Keys.F8;
        public static Keys ReBuildChunks = Keys.F9;

	    public static Keys ToggleWireframe = Keys.F10;
	}
}
