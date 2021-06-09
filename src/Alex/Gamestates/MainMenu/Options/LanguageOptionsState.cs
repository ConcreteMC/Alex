using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Alex.Common.Gui.Elements;
using Alex.Common.Localization;
using Alex.Gui;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options
{
    public class LanguageOptionsState : OptionsStateBase
    {
        private Dictionary<CultureLanguage, Button> _languageButtons = new Dictionary<CultureLanguage, Button>();
        private (Button button, CultureLanguage culture) _activeBtn;
        public LanguageOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
        {
            TitleTranslationKey = "options.language";
        }
        
        private static string GetTitleCaseNativeLanguage(CultureInfo culture)
        {
            string nativeName = culture.IsNeutralCulture
                ? culture.NativeName
                : culture.Parent.NativeName;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nativeName);
        }

        private string GetButtonText(CultureLanguage culture, bool active)
        {
            string displayName = culture.DisplayName;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = culture.Name;
            }

            return active ? $"[Active] {displayName}" : displayName;
        }

        private void SetLanguage(CultureLanguage culture)
        {
            if (_activeBtn.button != null && _activeBtn.culture != null)
            {
                _activeBtn.button.Text = GetButtonText(_activeBtn.culture, false);
            }

            if (_languageButtons.TryGetValue(culture, out Button btn))
            {
                btn.Text = GetButtonText(culture, true);
                //Alex.GuiManager.FocusManager.FocusedElement = btn;
                //FocusContext?.Focus(_activeBtn.button);
                
                _activeBtn = (btn, culture);
            }

            Options.MiscelaneousOptions.Language.Value = culture.Code;
            //Alex.GuiRenderer.SetLanguage(culture.CultureInfo.Name);
        }
        
        private bool _didInit = false;
        protected override void OnInit(IGuiRenderer renderer)
        {
            if (!_didInit)
            {
                _didInit = true;
                var activeLang = Alex.GuiRenderer.Language;

                foreach (var lng in Alex.GuiRenderer.Languages.OrderBy(x => x.Key))
                {
                    if (System.Text.Encoding.UTF8.GetByteCount(lng.Value.DisplayName) != lng.Value.DisplayName.Length
                    ) //Filter-out non-ascii languages
                        continue;

                    bool active = lng.Value.Code.Equals(activeLang.Code);

                    Button btn = new AlexButton(GetButtonText(lng.Value, active), () => { SetLanguage(lng.Value); })
                       .ApplyModernStyle(false);

                    _languageButtons.Add(lng.Value, btn);

                    AddGuiRow(btn);

                    if (active)
                    {
                        Alex.GuiManager.FocusManager.FocusedElement = btn;
                        _activeBtn = (btn, lng.Value);
                    }
                }
            }

            base.OnInit(renderer);
        }

        protected override void OnShow()
        {
            FocusContext?.Focus(_activeBtn.button);
            
            base.OnShow();
        }
    }
}
