using System;
using System.Collections.Generic;
using Alex.Common.Gui.Elements;
using Alex.Common.Utils;
using Alex.Gui.Elements;
using Alex.Interfaces;
using Microsoft.Xna.Framework;
using MiNET.Net;
using MiNET.UI;
using Newtonsoft.Json;
using RocketUI;
using RocketUI.Input;
using Button = RocketUI.Button;
using Slider = MiNET.UI.Slider;

namespace Alex.Gui.Forms
{
	public class CustomFormDialog : FormBase
	{
		//private Dictionary<>
		private StackContainer Header { get; }
		private CustomForm Form { get; }
		private Button SubmitButton { get; }

		public CustomFormDialog(uint formId, BedrockFormManager parent, CustomForm form, InputManager inputManager) :
			base(formId, parent, inputManager)
		{
			Form = form;

			ScrollableStackContainer stackContainer = new ScrollableStackContainer();
			stackContainer.Orientation = Orientation.Vertical;
			stackContainer.Anchor = Alignment.Fill;
			stackContainer.ChildAnchor = Alignment.MiddleFill;
			stackContainer.Background = Color.Black * 0.35f;
			var margin = new Thickness(5, 5);

			foreach (var element in form.Content)
			{
				switch (element)
				{
					case Label label:
					{
						stackContainer.AddChild(new TextElement() { Text = label.Text, Margin = margin });
					}

						break;

					case Input input:
					{
						TextInput guiInput = new TextInput()
						{
							Value = input.Value,
							PlaceHolder =
								!string.IsNullOrWhiteSpace(input.Placeholder) ? input.Placeholder : input.Text,
							Margin = margin
						};

						guiInput.ValueChanged += (sender, s) => { input.Value = s; };

						stackContainer.AddChild(guiInput);
					}

						break;

					case Toggle toggle:
					{
						ToggleButton guiToggle;

						stackContainer.AddChild(
							guiToggle = new ToggleButton(toggle.Text) { Margin = margin, Value = !toggle.Value }
							   .ApplyModernStyle(true));

						guiToggle.DisplayFormat =
							new ValueFormatter<bool>((val) => { return $"{toggle.Text}: {val.ToString()}"; });

						guiToggle.Value = toggle.Value;

						guiToggle.ValueChanged += (sender, b) => { toggle.Value = b; };
					}

						break;

					case Slider slider:
					{
						RocketUI.Slider Slider;

						stackContainer.AddChild(
							Slider = new RocketUI.Slider()
							{
								Label = { Text = slider.Text, },
								Value = slider.Value,
								MaxValue = slider.Max,
								MinValue = slider.Min,
								StepInterval = slider.Step,
								Margin = margin,
								DisplayFormat = (new ValueFormatter<double>($"{slider.Text}: {{0}}"))
							}.ApplyStyle());

						Slider.ValueChanged += (sender, d) => { slider.Value = (float)d; };
					}

						break;

					case StepSlider stepSlider:
					{
						RocketUI.Slider Slider;

						stackContainer.AddChild(
							Slider = new RocketUI.Slider()
							{
								Label = { Text = stepSlider.Text, },
								Value = stepSlider.Value,
								MaxValue = stepSlider.Steps.Count,
								MinValue = 0,
								StepInterval = 1,
								Margin = margin,
								DisplayFormat = (new ValueFormatter<double>(
									value =>
									{
										int v = (int)Math.Floor(value);
										var text = stepSlider.Steps[v];

										return text;
									}))
							}.ApplyStyle());

						Slider.ValueChanged += (sender, d) => { stepSlider.Value = (int)Math.Floor(d); };
					}

						break;

					case Dropdown dropdown:
					{
						GuiDropdown dd = new GuiDropdown() { Margin = margin };
						dd.Options.AddRange(dropdown.Options);

						dd.ValueChanged += (sender, i) => { dropdown.Value = i; };

						stackContainer.AddChild(dd);

						dd.Value = dropdown.Value;

						// stackContainer.AddChild(new TextElement()
						// {
						//     Text = "Unsupported dropdown",
						//    TextColor = (Color) TextColor.Red,
						//    Margin = margin
						// });
					}

						break;
				}
			}

			SubmitButton = new AlexButton("Submit", SubmitPressed);

			stackContainer.AddChild(SubmitButton);

			Background = Color.Transparent;

			var width = 356;
			var height = width;

			ContentContainer.Width = ContentContainer.MinWidth = ContentContainer.MaxWidth = width;
			ContentContainer.Height = ContentContainer.MinHeight = ContentContainer.MaxHeight = height;

			SetFixedSize(width, height);

			ContentContainer.AutoSizeMode = AutoSizeMode.None;

			Container.Anchor = Alignment.MiddleCenter;

			var bodyWrapper = new Container();
			bodyWrapper.Anchor = Alignment.Fill;
			bodyWrapper.Padding = new Thickness(5, 0);
			bodyWrapper.AddChild(stackContainer);

			Container.AddChild(bodyWrapper);

			Container.AddChild(
				Header = new StackContainer()
				{
					Anchor = Alignment.TopFill,
					ChildAnchor = Alignment.BottomCenter,
					Height = 32,
					Padding = new Thickness(3),
					Background = Color.Black * 0.5f
				});

			Header.AddChild(
				new TextElement()
				{
					Text = FixContrast(form.Title),
					TextColor = (Color)TextColor.White,
					Scale = 2f,
					FontStyle = FontStyle.DropShadow,
					Anchor = Alignment.BottomCenter,
				});

			stackContainer.Margin = new Thickness(0, Header.Height, 0, 0);
		}

		private string Serialize()
		{
			List<object> data = new List<object>();

			foreach (var element in Form.Content)
			{
				switch (element)
				{
					case Input input:
					{
						data.Add(input.Value);
					}

						break;

					case Toggle toggle:
					{
						data.Add(toggle.Value);
					}

						break;

					case Slider slider:
					{
						data.Add(slider.Value);
					}

						break;

					case StepSlider stepSlider:
					{
						data.Add(stepSlider.Value);
					}

						break;

					case Dropdown dropdown:
					{
						data.Add(dropdown.Value);
					}

						break;

					default:
						data.Add(null);

						break;
				}
			}

			return JsonConvert.SerializeObject(data.ToArray());
		}

		private void SubmitPressed()
		{
			Parent.Hide(FormId);

			var packet = McpeModalFormResponse.CreateObject();
			packet.formId = FormId;
			packet.data = Serialize();

			//JsonConvert.SerializeObject(idx)
			Parent.SendResponse(packet);

			//Submit.
		}
	}
}