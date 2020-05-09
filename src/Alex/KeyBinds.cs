using System.Collections.Generic;
using Alex.API.Input;
using Microsoft.Xna.Framework.Input;

namespace Alex
{
	//TODO: Get rid of this completely
    public static class KeyBinds
    {
	    public static Keys Fog = Keys.F;
	    public static Keys DebugInfo = Keys.F3;
		public static Keys[] EntityBoundingBoxes = new Keys[]
		{
			DebugInfo, Keys.B
		};

		public static Keys[] NetworkDebugging = new Keys[]
		{
			DebugInfo, Keys.N
		};
		
		public static Keys[] AlwaysDay = new Keys[]
		{
			DebugInfo, Keys.D
		};

	    public static Keys ChangeCamera = Keys.F5;
	    public static Keys ReBuildChunks = Keys.F9;

	    public static Keys ToggleWireframe = Keys.F10;
	    
	    public static readonly IReadOnlyDictionary<InputCommand, Keys> DefaultBindings = new Dictionary<InputCommand, Keys>()
	    {
		    {InputCommand.Exit, Keys.Escape},
            
		    {InputCommand.MoveForwards, Keys.W},
		    {InputCommand.MoveBackwards, Keys.S},
		    {InputCommand.MoveLeft, Keys.A},
		    {InputCommand.MoveRight, Keys.D},
		    {InputCommand.MoveUp, Keys.Space},
		    {InputCommand.MoveDown, Keys.LeftShift},

		    {InputCommand.MoveSpeedIncrease, Keys.OemPlus},
		    {InputCommand.MoveSpeedDecrease, Keys.OemMinus},
		    {InputCommand.MoveSpeedReset, Keys.OemPeriod},

		    {InputCommand.ToggleFog, Keys.F},
		    {InputCommand.ToggleDebugInfo, Keys.F3},
		    {InputCommand.ToggleChat, Keys.T},

		    {InputCommand.ToggleCamera, Keys.F5},
		    {InputCommand.ToggleCameraFree, Keys.F8},
		    {InputCommand.RebuildChunks, Keys.F9},
		    {InputCommand.ToggleWireframe, Keys.F10},

		    {InputCommand.ToggleInventory, Keys.E},

            
		    {InputCommand.HotBarSelect1, Keys.D1},
		    {InputCommand.HotBarSelect2, Keys.D2},
		    {InputCommand.HotBarSelect3, Keys.D3},
		    {InputCommand.HotBarSelect4, Keys.D4},
		    {InputCommand.HotBarSelect5, Keys.D5},
		    {InputCommand.HotBarSelect6, Keys.D6},
		    {InputCommand.HotBarSelect7, Keys.D7},
		    {InputCommand.HotBarSelect8, Keys.D8},
		    {InputCommand.HotBarSelect9, Keys.D9},

		    {InputCommand.Right, Keys.Right},
		    {InputCommand.Left, Keys.Left},
		    {InputCommand.Up, Keys.Up},
		    {InputCommand.Down, Keys.Down}
	    };
	}
}
