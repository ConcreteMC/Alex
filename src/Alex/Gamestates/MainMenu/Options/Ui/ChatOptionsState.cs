using Alex.Gui;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options.Ui;

public class ChatOptionsState : OptionsStateBase
{
	private ToggleButton Enabled            { get; set; }
	private Slider MessageHistory { get; set; }
	
	/// <inheritdoc />
	public ChatOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
	{
		Title = "Chat";
	}
	
	/// <inheritdoc />
	protected override void Initialize(IGuiRenderer renderer)
	{
		base.Initialize(renderer);
		Enabled = CreateToggle("Visible: {0}", a => a.UserInterfaceOptions.Chat.Enabled);
		MessageHistory = CreateSlider(
			"Message History: {0}", o => o.UserInterfaceOptions.Chat.MessageHistory, 0, 100, 10);

		AddGuiRow(Enabled, MessageHistory);
			
		AddDescription(Enabled, "Visible", "Enable/disable chat");

		AddDescription(
			MessageHistory, "Message History", "Determines the amount of chat messages to keep in memory.");
	}
}