using System;
using Alex.Gui.Elements.Web;
using RocketUI;

namespace Alex.Gui.Dialogs
{
	public class BrowserDialog : DialogBase
	{
		private WebElement _webElement;
		public BrowserDialog()
		{
			Anchor = Alignment.Fill;
			
			AddChild(_webElement = new WebElement()
			{
				Source = new Uri("https://google.com"),
				Anchor = Alignment.Fill,
				Margin = new Thickness(5, 5)
			});
		}

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
			_webElement.Focus();
		}
	}
}