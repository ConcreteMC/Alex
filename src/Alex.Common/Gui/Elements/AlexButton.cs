using System;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Common.Gui.Elements
{
	public class AlexButton : Button
	{
		public AlexButton(Action action = null) : this(string.Empty, action) { }

		public AlexButton(string text, Action action = null, bool isTranslationKey = false) : base(
			text, action, isTranslationKey)
		{
			IsModern = true;
		}

		private bool _isModern = false;

		public bool IsModern
		{
			get
			{
				return _isModern;
			}
			set
			{
				if (value)
				{
					if (!ClassNames.Contains("Modern"))
						ClassNames.Add("Modern");
				}
				else
				{
					if (ClassNames.Contains("Modern"))
						ClassNames.Remove("Modern");
				}
			}
		}
	}
}