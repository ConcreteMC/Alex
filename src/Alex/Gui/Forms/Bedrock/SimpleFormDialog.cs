using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Dialogs;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using MiNET.Net;
using MiNET.UI;
using Newtonsoft.Json;
using RocketUI;

namespace Alex.Gui.Forms.Bedrock
{
    public class SimpleFormDialog : GuiDialogBase
    {
        private BedrockFormManager Parent { get; }
        private GuiStackMenu StackMenu { get; }
        public SimpleFormDialog(uint formId, BedrockFormManager parent, GuiManager guiManager, SimpleForm form)
        {
            Parent = parent;

            Background = new Color(Color.Black, 0.5f);
            
            GuiContainer container = new GuiContainer();
            container.Anchor = Alignment.Fill;
            
            StackMenu = new GuiStackMenu();
            StackMenu.Anchor = Alignment.MiddleCenter;

            if (!string.IsNullOrWhiteSpace(form.Content))
            {
                StackMenu.AddMenuItem(form.Content, () => {}, false);
                StackMenu.AddSpacer();
            }
            
            for (var index = 0; index < form.Buttons.Count; index++)
            {
                var button = form.Buttons[index];
                int idx = index;
                
                StackMenu.AddMenuItem(button.Text, () =>
                {
                    parent.SendResponse(new McpeModalFormResponse()
                    {
                        formId = formId,
                        data = JsonConvert.SerializeObject((int?) idx)
                    });
                    guiManager.HideDialog(this);
                });
            }
            
            container.AddChild(StackMenu);
            
            container.AddChild(new GuiTextElement()
            {
                Anchor = Alignment.TopCenter,
                Text = form.Title,
                FontStyle = FontStyle.Bold,
                Scale = 2f,
                TextColor = TextColor.White
            });
            
            AddChild(container);
        }
    }
}