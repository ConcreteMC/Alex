using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;
using System;
using Alex.API.Graphics.Typography;

namespace Alex.GameStates.Gui.Common
{
    public class GuiMenuStateBase : GuiGameStateBase
    {
        public virtual int BodyMinWidth { get; set; } = 356;

        private string _title;
        private string _titleTranslationKey;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                if(_headerTitle != null)
                    _headerTitle.Text = value;
            }
        }
        public string TitleTranslationKey
        {
            get => _titleTranslationKey;
            set
            {
                _titleTranslationKey = value;
                if(_headerTitle != null)
                    _headerTitle.TranslationKey = value;
            }
        }

        protected GuiStackContainer      Header { get; }
        protected GuiMultiStackContainer Body { get; }
        protected GuiMultiStackContainer Footer { get; }

        private readonly GuiTextElement  _headerTitle;

        public GuiMenuStateBase()
        {
            Background = new GuiTexture2D
            { 
                TextureResource = GuiTextures.OptionsBackground, 
                RepeatMode = TextureRepeatMode.Tile,
                Scale =  new Vector2(2f, 2f),
            };
            BackgroundOverlay = new Color(Color.Black, 0.65f);
            

            AddChild(Header = new GuiStackContainer()
            {
                Height              = 32,
                Padding = new Thickness(3),
                Margin = new Thickness(3, 3, 3, 6),

                Anchor = Alignment.TopFill,
                ChildAnchor = Alignment.BottomCenter
            });
            
            Header.AddChild(_headerTitle = new GuiTextElement()
            {
                Text      = Title,
                TextColor = TextColor.White,
                Scale     = 1f,
                FontStyle = FontStyle.DropShadow,
                
                Anchor = Alignment.BottomCenter,
            });

            AddChild(Footer = new GuiMultiStackContainer(row =>
                         {
                             row.Anchor = Alignment.BottomFill;
                             //row.Orientation = Orientation.Horizontal;
                             row.ChildAnchor = Alignment.BottomFill;
                             //row.Margin = new Thickness(3);
                             row.Width = BodyMinWidth;
                             row.MaxWidth = BodyMinWidth;
                         })
            {
                Height  = 64,

                Orientation = Orientation.Vertical,
                Anchor = Alignment.BottomFill,
                ChildAnchor = Alignment.TopCenter
            });

	        AddChild(Body = new GuiMultiStackContainer(row =>
	                     {
	                         row.ChildAnchor = Alignment.MiddleFill;
                             row.Margin = new Thickness(3);
	                         row.Width = BodyMinWidth;
	                         row.MinWidth = BodyMinWidth;
	                     })
	        {
	            Margin = new Thickness(0, Header.Height, 0, Footer.Height),
                
	            Orientation = Orientation.Vertical,
		        Anchor = Alignment.Fill,
		        ChildAnchor = Alignment.FillCenter,
	        });
		}

		protected TGuiElement AddGuiElement<TGuiElement>(TGuiElement element) where TGuiElement : IGuiElement
        {
            Body.AddChild(element);

            return element;
        }

        protected GuiStackContainer AddGuiRow(params GuiElement[] elements)
        {
            return Body.AddRow(row =>
            {
                foreach (var element in elements)
                {
                    row.AddChild(element);
                }
            });
        }
    }
}
