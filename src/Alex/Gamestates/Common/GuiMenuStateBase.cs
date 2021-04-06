
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI;


namespace Alex.Gamestates.Common
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
                if(HeaderTitle != null)
                    HeaderTitle.Text = value;
            }
        }
        public string TitleTranslationKey
        {
            get => _titleTranslationKey;
            set
            {
                _titleTranslationKey = value;
                if(HeaderTitle != null)
                    HeaderTitle.TranslationKey = value;
            }
        }

        protected StackContainer      Header { get; }
        protected ScrollableMultiStackContainer Body { get; }
        protected MultiStackContainer Footer { get; }

        protected readonly TextElement  HeaderTitle;

        public GuiMenuStateBase()
        {
            Background = new GuiTexture2D
            { 
                TextureResource = AlexGuiTextures.OptionsBackground, 
                RepeatMode = TextureRepeatMode.Tile,
                Scale =  new Vector2(2f, 2f),
            };
            BackgroundOverlay = new Color(Color.Black, 0.65f);
            

            AddChild(Header = new StackContainer()
            {
                Height              = 32,
                Padding = new Thickness(3),
                Margin = new Thickness(3, 3, 3, 6),

                Anchor = Alignment.TopFill,
                ChildAnchor = Alignment.BottomCenter
            });
            
            Header.AddChild(HeaderTitle = new TextElement()
            {
                Text      = Title,
                TextColor = (Color) TextColor.White,
                Scale     = 1f,
                FontStyle = FontStyle.DropShadow | FontStyle.Bold,
                
                Anchor = Alignment.BottomCenter,
            });

			AddChild(Body = new ScrollableMultiStackContainer(row =>
			{
				row.ChildAnchor = Alignment.MiddleFill;
				row.Margin = new Thickness(3);
				row.Width = BodyMinWidth;
				row.MinWidth = BodyMinWidth;
			})
			{
				//Margin = new Thickness(0, Header.Height, 0, Footer.Height),
                //AutoSizeMode = AutoSizeMode.None,
                //Height = 100,
                //MaxHeight = 100,
				Orientation = Orientation.Vertical,
				Anchor = Alignment.Fill,
				ChildAnchor = Alignment.TopCenter,
			});

			AddChild(Footer = new MultiStackContainer(row =>
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

			Body.Margin = new Thickness(0, Header.Height, 0, Footer.Height);
		}

		protected TRocketElement AddRocketElement<TRocketElement>(TRocketElement element) where TRocketElement : IGuiElement
        {
            Body.AddChild(element);

            return element;
        }

        protected StackContainer AddGuiRow(params RocketElement[] elements)
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
