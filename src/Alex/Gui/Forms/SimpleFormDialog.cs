using System;
using Alex.Common.Gui.Elements;
using Microsoft.Xna.Framework;
using MiNET.Net;
using MiNET.UI;
using RocketUI;
using RocketUI.Input;


namespace Alex.Gui.Forms
{
	public class SimpleFormDialog : FormBase
	{
		private StackContainer Header { get; }
		//public  MultiStackContainer Footer { get; }

		private StackMenu StackMenu { get; }

		public SimpleFormDialog(uint formId, BedrockFormManager parent, SimpleForm form, InputManager inputManager) :
			base(formId, parent, inputManager)
		{
			var width = 356;

			StackMenu = new StackMenu();
			StackMenu.Anchor = Alignment.Fill;
			StackMenu.ChildAnchor = Alignment.MiddleCenter;
			StackMenu.Background = Color.Black * 0.35f;

			if (!string.IsNullOrWhiteSpace(form.Content))
			{
				var label = new GuiStackMenuLabel(FixContrast(form.Content)) { MaxWidth = width, };
				label.Wrap = true;
				StackMenu.AddChild(label);
				StackMenu.AddSpacer();
			}

			var btns = form.Buttons.ToArray();

			for (var index = 0; index < btns.Length; index++)
			{
				var button = btns[index];
				int idx = index;

				Action submitAction = () =>
				{
					var packet = McpeModalFormResponse.CreateObject();
					packet.formId = formId;
					packet.data = idx.ToString();
					//JsonConvert.SerializeObject(idx)
					parent.SendResponse(packet);

					parent.Hide(formId);
				};

				if (button.Image != null)
				{
					switch (button.Image.Type)
					{
						case "url":
							StackMenu.AddChild(new FormImageButton(button.Image.Url, button.Text, submitAction));

							continue;

							break;

						case "path":
							break;
					}
				}

				StackMenu.AddChild(
					new StackMenuItem(button.Text, submitAction)
					{
						Enabled = true, TextAlignment = TextAlignment.Center
					}.ApplyModernStyle(true));
				// StackMenu.AddMenuItem(button.Text, submitAction);
			}

			Background = Color.Transparent;
			var height = width;

			ContentContainer.Width = ContentContainer.MinWidth = ContentContainer.MaxWidth = width;
			ContentContainer.Height = ContentContainer.MinHeight = ContentContainer.MaxHeight = height;

			SetFixedSize(width, height);

			ContentContainer.AutoSizeMode = AutoSizeMode.None;

			Container.Anchor = Alignment.MiddleCenter;

			var bodyWrapper = new Container();
			bodyWrapper.Anchor = Alignment.Fill;
			bodyWrapper.Padding = new Thickness(5, 0);
			bodyWrapper.AddChild(StackMenu);

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
					TextColor = Color.White,
					Scale = 2f,
					FontStyle = FontStyle.DropShadow,
					Anchor = Alignment.BottomCenter,
				});

			StackMenu.Margin = new Thickness(0, Header.Height, 0, 0);
		}
	}
}