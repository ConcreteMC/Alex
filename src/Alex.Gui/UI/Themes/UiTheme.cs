using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using log4net;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.UI.Themes
{
    public delegate bool UiElementPredicate<TUiElement>(TUiElement element) where TUiElement : UiElement;

    public delegate bool UiElementPredicate(UiElement element);

    public class UiTheme
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UiTheme));
        
        private List<Texture2D> _textures = new List<Texture2D>();
        private List<UiThemeStyleSheet> _styleSheets = new List<UiThemeStyleSheet>();
        
        public void AddClass<TUiElement>(UiElementStyle style) where TUiElement : UiElement
        {
            AddStyle(e => (e is TUiElement), style);
        }

        public void AddClass<TUiElement>(UiElementPredicate<TUiElement> predicate, UiElementStyle style) where TUiElement : UiElement
        {
            AddStyle(MakePredicate(predicate), style);
        }

        public void AddClass(string className, UiElementStyle style)
        {
            AddStyle(ClassPredicate(className), style);
        }
        
        private void AddStyle(UiElementPredicate predicate, UiElementStyle style)
        {
            _styleSheets.Add(new UiThemeStyleSheet(predicate, style));
        }

        public UiElementStyle GetCompiledStyleFor(UiElement element)
        {
            return MergeStyles(GetStylesFor(element));
        }

        public UiElementStyle[] GetStylesFor(UiElement element)
        {
            return _styleSheets.Where(ss => ss.Predicate(element)).Select(ss => ss.Style).ToArray();
        }

        public void ClearStyles()
        {
            _styleSheets.Clear();
        }

        private UiElementPredicate MakePredicate<TUiElement>(UiElementPredicate<TUiElement> predicate) where TUiElement : UiElement
        {
            return e => (e is TUiElement uiElement) && predicate(uiElement);
        }

        private UiElementPredicate ClassPredicate(string className)
        {
            return e =>
            {
                if (string.IsNullOrWhiteSpace(className) && string.IsNullOrWhiteSpace(e.ClassName))
                {
                    return true;
                }

                return e.ClassName?.Equals(className, StringComparison.InvariantCultureIgnoreCase) ?? false;
            };
        }

        private static readonly MapperConfiguration _mapperConfiguration;
        private static readonly IMapper _mapper;
        static UiTheme()
        {
            _mapperConfiguration = new MapperConfiguration(c => { c.CreateMap<UiElementStyle, UiElementStyle>().ForAllMembers(m => m.Condition((src, dest, srcMember) => srcMember != null)); });
            _mapper = _mapperConfiguration.CreateMapper();
        }

        public static UiElementStyle MergeStyles(params UiElementStyle[] styles)
        {
            var mergedStyle = new UiElementStyle();

            if (styles != null && styles.Any())
            {
                foreach (var style in styles)
                {
                    _mapper.Map(style, mergedStyle);
                }
            }

            return mergedStyle;
        }

    }
}
