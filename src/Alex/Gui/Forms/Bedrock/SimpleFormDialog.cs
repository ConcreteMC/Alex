using System;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Utils;
using MiNET.Net;
using MiNET.UI;
using RocketUI;

namespace Alex.Gui.Forms.Bedrock
{
    public class SimpleFormDialog : FormBase
    {
        private GuiStackMenu StackMenu { get; }
        public SimpleFormDialog(uint formId, BedrockFormManager parent, SimpleForm form, InputManager inputManager) : base(formId, parent, inputManager)
        {
            StackMenu = new GuiStackMenu();
            StackMenu.Anchor = Alignment.Fill;
            StackMenu.ChildAnchor = Alignment.MiddleFill;
            
            if (!string.IsNullOrWhiteSpace(form.Content))
            {
                StackMenu.AddMenuItem(form.Content, () => {}, false);
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
                
                StackMenu.AddMenuItem(button.Text, submitAction);
            }
            
            Container.AddChild(StackMenu);
            
            Container.AddChild(new GuiTextElement()
            {
                Anchor = Alignment.TopCenter,
                Text = form.Title,
                FontStyle = FontStyle.Bold,
                Scale = 2f,
                TextColor = TextColor.White
            });
            
            AddChild(Container);
        }
    }
}