using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Alex
{
	//TODO: Get rid of this completely
	public static class KeyBinds
	{
		public static Keys Fog = Keys.F;
		public static Keys DebugInfo = Keys.F3;

		public static Keys[] AlwaysDay = new Keys[] { DebugInfo, Keys.D };

		public static Keys ChangeCamera = Keys.F5;
		public static Keys ReBuildChunks = Keys.F9;
	}
}