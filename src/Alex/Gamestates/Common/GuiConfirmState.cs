using System;
using Alex.Common.Gui.Elements;
using RocketUI;

namespace Alex.Gamestates.Common
{
	public class GuiConfirmState : GuiCallbackStateBase<bool>
	{
		private readonly GuiConfirmStateOptions _options;

		public class GuiConfirmStateOptions
		{
			public string MessageText { get; set; }
			public string MessageTranslationKey { get; set; }

			public string ConfirmText { get; set; } = "Yes";
			public string ConfirmTranslationKey { get; set; } = "gui.yes";

			public string CancelText { get; set; } = "No";
			public string CancelTranslationKey { get; set; } = "gui.no";

			public string WarningText { get; set; } = null;
			public string WarningTranslationKey { get; set; } = null;

			public object?[] WarningParameters { get; set; } = Array.Empty<string>();
		}

		public GuiConfirmState(string message, Action<bool> callbackAction) : this(
			new GuiConfirmStateOptions() { MessageText = message }, callbackAction) { }

		public GuiConfirmState(string message, string messageTranslationKey, Action<bool> callbackAction) : this(
			new GuiConfirmStateOptions() { MessageText = message, MessageTranslationKey = messageTranslationKey },
			callbackAction) { }

		private TextElement _warningElement = null;

		public GuiConfirmState(GuiConfirmStateOptions options, Action<bool> callbackAction) : base(callbackAction)
		{
			_options = options;

			Body.Anchor = Alignment.MiddleCenter;

			AddRocketElement(
				new TextElement() { Text = options.MessageText, TranslationKey = options.MessageTranslationKey });

			if (!string.IsNullOrWhiteSpace(options.WarningText)
			    || !string.IsNullOrWhiteSpace(options.WarningTranslationKey))
			{
				AddRocketElement(
					_warningElement =
						new TextElement() { Text = options.WarningText ?? options.WarningTranslationKey });
			}

			var row = AddGuiRow(
				new AlexButton("Confirm", OnConfirmButtonPressed)
				{
					Text = options.ConfirmText, TranslationKey = options.ConfirmTranslationKey
				},
				new AlexButton("Cancel", OnCancelButtonPressed)
				{
					Text = options.CancelText, TranslationKey = options.CancelTranslationKey
				});

			row.Orientation = Orientation.Horizontal;
		}

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);

			if (_warningElement != null)
			{
				string text = renderer.GetTranslation(_options.WarningTranslationKey) ?? _options.WarningText;
				_warningElement.Text = string.Format(PrepareString(text), _options.WarningParameters);
			}
		}

		private string PrepareString(string input)
		{
			int c = 0;

			for (int index = input.IndexOf('%'); index >= 0; index = input.IndexOf('%'))
			{
				if (index + 1 < input.Length)
				{
					input = input.Remove(index, 2);
					input = input.Insert(index, $"{{{c++}}}");
				}
			}

			return input;
		}

		private void OnConfirmButtonPressed()
		{
			InvokeCallback(true);
		}

		private void OnCancelButtonPressed()
		{
			InvokeCallback(false);
		}
	}
}