using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Input;
using Alex.API.Utils;
using Alex.Gamestates.Multiplayer;
using Microsoft.Xna.Framework;
using MiNET.Net;
using MiNET.UI;
using RocketUI;
using FontStyle = Alex.API.Graphics.Typography.FontStyle;

namespace Alex.Gui.Forms
{
	public class ModalFormDialog : FormBase
	{
		private StackContainer      Header { get; }
		public  StackContainer      Body   { get; }
		public  MultiStackContainer Footer { get; }
		
		/// <inheritdoc />
		public ModalFormDialog(uint formId, BedrockFormManager parent, ModalForm form, InputManager inputManager) : base(
			formId, parent, inputManager)
		{
			Background = Color.Transparent;

			var width  = 356;
			var height = width;
			
			ContentContainer.Width = ContentContainer.MinWidth = ContentContainer.MaxWidth = width;
			ContentContainer.Height = ContentContainer.MinHeight = ContentContainer.MaxHeight = height;
            
			SetFixedSize(width, height);
            
			ContentContainer.AutoSizeMode = AutoSizeMode.None;
			
			Container.Anchor = Alignment.MiddleCenter;

			Container.AddChild(Footer = new MultiStackContainer(row =>
			{
				row.Anchor = Alignment.BottomFill;
				//row.Orientation = Orientation.Horizontal;
				row.ChildAnchor = Alignment.BottomFill;
				//row.Margin = new Thickness(3);
				row.Width = 356;
				row.MaxWidth = 356;
			})
			{
				Height  = 24,

				Orientation = Orientation.Vertical,
				Anchor = Alignment.BottomFill,
				ChildAnchor = Alignment.BottomCenter,
				Background = Color.Black * 0.5f
			});
			
			Footer.AddRow(row =>
			{
				row.AddChild(new Button(form.Button1, () =>
				{
					var packet = McpeModalFormResponse.CreateObject();
					packet.formId = formId;
					packet.data = "true";
					//JsonConvert.SerializeObject(idx)
					parent.SendResponse(packet);
                    
					parent.Hide(formId);
				})
				{
					Enabled = true,
					
				});
				row.AddChild(new Button(form.Button2, () =>
				{
					var packet = McpeModalFormResponse.CreateObject();
					packet.formId = formId;
					packet.data = "false";
					//JsonConvert.SerializeObject(idx)
					parent.SendResponse(packet);
                    
					parent.Hide(formId);
				})
				{
					Enabled = true
				});
			});
			
			Container.AddChild(Body = new StackContainer()
			{
				//Margin = new Thickness(0, Header.Height, 0, Footer.Height),
				//AutoSizeMode = AutoSizeMode.None,
				//Height = 100,
				//MaxHeight = 100,
				Orientation = Orientation.Vertical,
				Anchor = Alignment.Fill,
				ChildAnchor = Alignment.MiddleCenter,
				Background = Color.Black * 0.35f
				//HorizontalScrollMode = ScrollMode.Hidden
			});

			var text = form.Content;
					
			var newString = "";
			//	char[] help = new[] { '.', '?', '!' };
			for (int i = 0; i < text.Length - 1; i++)
			{
				var c = text[i];
				newString += c;
						
				if (c == '.' || c == '?' || c == '!')
				{
					//newString += "\n";
					
							Body.AddChild(new TextElement(newString));
							/*row.AddChild(new TextElement(form.Content)
							{
								Wrap = true,
								MaxWidth = 320
							});*/
					
					
					newString = "";

					if (i != text.Length - 1 && text[i + 1] == ' ')
						i++;
				}
			}
			
			if (newString.Length > 0)
				Body.AddChild(new TextElement(newString));

			Container.AddChild(Header = new StackContainer()
			{
				Anchor = Alignment.TopFill,
				ChildAnchor = Alignment.BottomCenter,
				Height = 32,
				Padding = new Thickness(3),
				Background = Color.Black * 0.5f
			});
			
			Header.AddChild(new TextElement()
			{
				Text      = FixContrast(form.Title),
				TextColor = TextColor.White,
				Scale     = 2f,
				FontStyle = FontStyle.DropShadow,
                
				Anchor = Alignment.BottomCenter,
			});
			
			Body.Margin = new Thickness(0, Header.Height, 0, Footer.Height);
		//	Body.MaxHeight = Body.Height = height - 64;
		}
	}
}