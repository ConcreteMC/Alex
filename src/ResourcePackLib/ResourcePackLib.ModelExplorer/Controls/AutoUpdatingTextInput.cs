using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;

namespace ResourcePackLib.ModelExplorer.Controls;

public class AutoUpdatingTextInput : TextInput
{
    private readonly Func<string> _getFn;
    private readonly Action<string> _setFn;

    public bool UpdateOnlyWhenNotFocused { get; set; } = true;
    public TimeSpan UpdateFrequency { get; set; } = TimeSpan.FromSeconds(0.5);

    private TimeSpan _lastUpdate = TimeSpan.Zero;
    
    public AutoUpdatingTextInput(Func<string> getFn, Action<string> setFn = null)
    {
        _getFn = getFn;
        _setFn = setFn;
    }
    
    protected override void OnUpdate(GameTime gameTime)
    {
        base.OnUpdate(gameTime);

        if (!Focused || !UpdateOnlyWhenNotFocused)
        {
            if (gameTime.TotalGameTime - _lastUpdate >= UpdateFrequency)
            {
                Value = _getFn();
                _lastUpdate = gameTime.TotalGameTime;
                
            }
        }
    }

    protected override bool OnKeyInput(char character, Keys key)
    {
        if (key == Keys.Enter || key == Keys.Tab)
        {
            _setFn?.Invoke(TextBuilder.Text);
            GuiManager.FocusManager.FocusedElement = null;
            return true;
        }
        
        return base.OnKeyInput(character, key);
    }
}