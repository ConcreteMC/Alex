using System.Collections.Generic;
using System.Linq;
using Alex.Common.Gui.Elements;
using Alex.Common.Input;
using RocketUI;
using Alex.Gui;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI.Input;
using RocketUI.Input.Listeners;

namespace Alex.Gamestates.MainMenu.Options
{
	public class ControlOptionsState : OptionsStateBase
	{
		private AlexKeyboardInputListener InputListener { get; set; }
		private GamePadInputListener ControllerInputListener { get; set; }
		private MouseInputListener MouseInputListener { get; set; }
		private Dictionary<string, KeybindElement> Inputs { get; } = new Dictionary<string, KeybindElement>();

		public ControlOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
		{
			TitleTranslationKey = "controls.title";

			Footer.AddRow(
				new AlexButton(ResetControls) { TranslationKey = "controls.reset", Anchor = Alignment.TopFill, }
				   .ApplyModernStyle(false));
		}

		private void ResetControls()
		{
			var inputListener = InputListener;

			if (inputListener == null)
				return;

			inputListener.ClearMap();
			AlexKeyboardInputListenerFactory.RegisterDefaults(inputListener);

			var inputs = Inputs.Values.ToArray();

			foreach (var input in inputs)
			{
				if (inputListener.ButtonMap.TryGetValue(input.InputCommand, out var value))
				{
					input.Value = value.Count > 0 ? value[0] : KeybindElement.Unbound;
				}
			}

			//Inputs.Clear();

			//AddInputs();
		}

		protected override void OnHide()
		{
			base.OnHide();

			Alex.Storage.TryWriteJson("controls", InputListener.ButtonMap);
		}

		private void AddInputs()
		{
			var keyboardInputListener = InputListener;
			var mouseInputListener = MouseInputListener;
			var gamepadInputListener = ControllerInputListener;

			foreach (var wrapper in AlexInputCommand.GetAll())
			{
				if (wrapper.BindingType != InputCommandType.Keyboard)
					continue;

				InputCommand inputCommand = wrapper.InputCommand;

				List<Keys[]> value = new List<Keys[]>();

				if (keyboardInputListener.ButtonMap.TryGetValue(inputCommand, out var keys))
				{
					value = keys;
				}
				//else if (mouseInputListener.ButtonMap.TryGetValue(inputCommand, out var mouseButton))
				//{
				// value.Add(mouseButton == MouseButton.Left ? keys);
				//}

				KeybindElement textInput;

				string translationKey = string.IsNullOrWhiteSpace(wrapper.TranslationKey) ? inputCommand.ToString() :
					wrapper.TranslationKey;

				var root = new RocketElement();
				root.Anchor = Alignment.Fill;
				root.AddChild(new TextElement() { TranslationKey = translationKey, Anchor = Alignment.TopLeft });

				root.AddChild(
					textInput = new KeybindElement(
						keyboardInputListener, inputCommand, value.Count > 0 ? value[0] : KeybindElement.Unbound)
					{
						Anchor = Alignment.TopRight, Width = 120
					});

				if (value.Any(x => x.Any(b => b == Keys.Escape)))
				{
					textInput.ReadOnly = true;
				}

				var row = AddGuiRow(root);
				row.Margin = new Thickness(5, 0);

				textInput.ValueChanged += (sender, newValue) =>
				{
					if (newValue == KeybindElement.Unbound)
					{
						keyboardInputListener.RemoveMap(inputCommand);
					}
					else
					{
						foreach (var input in Inputs.Where(x => x.Key != inputCommand && x.Value.Value == newValue))
						{
							input.Value.Value = KeybindElement.Unbound;
						}

						InputListener.RegisterMap(inputCommand, newValue);
					}

					base.Alex.GuiManager.FocusManager.FocusedElement = null;

					textInput.ClearFocus();
					value.Clear();
					value.Add(newValue);
				};

				Inputs.TryAdd(inputCommand, textInput);
			}
		}

		/*private void AddInputs()
		{
		   if (InputListener != null)
		       AddInputs(InputListener);
		   
		   if (ControllerInputListener != null)
		       AddInputs(ControllerInputListener);

		   if (MouseInputListener != null)
		       AddInputs(MouseInputListener);
		}*/

		/// <inheritdoc />
		protected override void Initialize(IGuiRenderer renderer)
		{
			base.Initialize(renderer);

			AddGuiRow(
				new TextElement("Cursor Options")
				{
					TextAlignment = TextAlignment.Center, FontStyle = FontStyle.Underline
				});

			AddGuiRow(
				CreateToggle("Invert X: {0}", o => o.ControllerOptions.InvertX),
				CreateToggle("Invert Y: {0}", o => o.ControllerOptions.InvertY));

			AddGuiRow(new StackMenuSpacer());

			AddGuiRow(
				new TextElement("Keybinds") { TextAlignment = TextAlignment.Center, FontStyle = FontStyle.Underline });

			var inputManager = base.Alex.InputManager.GetOrAddPlayerManager(PlayerIndex.One);

			if (inputManager.TryGetListener(out AlexKeyboardInputListener inputListener))
			{
				InputListener = inputListener;
			}

			if (inputManager.TryGetListener(out GamePadInputListener controller))
			{
				ControllerInputListener = controller;
			}

			if (inputManager.TryGetListener(out MouseInputListener mouse))
			{
				MouseInputListener = mouse;
			}

			AddInputs();
		}

		private MouseState _previousState = new MouseState();

		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			var guiManager = GuiManager;
			var focusManager = guiManager?.FocusManager;

			if (focusManager == null)
				return;

			var mouseState = Mouse.GetState();

			if (mouseState != _previousState)
			{
				if (mouseState.LeftButton == ButtonState.Released && _previousState.LeftButton == ButtonState.Pressed)
				{
					if (focusManager.FocusedElement is KeybindElement keybindElement
					    && !keybindElement.RenderBounds.Contains(GuiRenderer.Unproject(mouseState.Position.ToVector2()))
					    && keybindElement.IsChanging)
					{
						focusManager.FocusedElement = null;
					}
				}
			}

			_previousState = mouseState;
		}
	}
}