using System;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Dialogs;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using MiNET.Net;
using MiNET.UI;
using Newtonsoft.Json;
using RocketUI;

namespace Alex.Gui.Forms.Bedrock
{
    public class FormBase : GuiDialogBase
    {
        public uint FormId { get; set; }
        protected BedrockFormManager Parent { get; }
        protected InputManager InputManager { get; }
        public FormBase(uint formId, BedrockFormManager parent, InputManager inputManager)
        {
            FormId = formId;
            Parent = parent;
            InputManager = inputManager;
        }    
        
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (InputManager.Any(x => x.IsPressed(InputCommand.Exit)))
            {
                Parent.Hide(FormId);
            }
        }
    }
    
    public class SimpleFormDialog : FormBase
    {
        private GuiStackMenu StackMenu { get; }
        public SimpleFormDialog(uint formId, BedrockFormManager parent, SimpleForm form, InputManager inputManager) : base(formId, parent, inputManager)
        {
            Background = new Color(Color.Black, 0.5f);
            
            GuiContainer container = new GuiContainer();
            container.Anchor = Alignment.FillCenter;
            
            StackMenu = new GuiStackMenu();
            StackMenu.Anchor = Alignment.FillCenter;
            StackMenu.ChildAnchor = Alignment.CenterY | Alignment.FillX;
            
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