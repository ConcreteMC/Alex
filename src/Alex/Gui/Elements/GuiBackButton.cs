using Alex.Common.Gui.Elements;

namespace Alex.Gui.Elements
{
	public class GuiBackButton : AlexButton
	{
		public GuiBackButton() : base("Back", () => Alex.Instance.GameStateManager.Back()) { }
	}
}