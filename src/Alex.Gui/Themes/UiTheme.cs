using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.Gui.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Themes
{
    public delegate bool UiElementPredicate<TUiElement>(TUiElement element) where TUiElement : UiElement;

    public delegate bool UiElementPredicate(UiElement element);

    public class UiTheme
    {
        private List<Texture2D> _textures = new List<Texture2D>();
        private List<UiThemeStyleSheet> _styleSheets = new List<UiThemeStyleSheet>();

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
            var styles = GetStylesFor(element);

            var elementStyle = new UiElementStyle();

            foreach (var style in styles)
            {
                elementStyle.ApplyStyle(style);
            }

            return elementStyle;
        }

        public UiElementStyle[] GetStylesFor(UiElement element)
        {
            return _styleSheets.Where(ss => ss.Predicate(element)).Select(ss => ss.Style).OrderByDescending(s => s.Priority).ToArray();
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

    }
}
