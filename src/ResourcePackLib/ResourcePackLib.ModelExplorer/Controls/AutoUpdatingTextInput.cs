using Microsoft.Xna.Framework;
using RocketUI;

namespace ResourcePackLib.ModelExplorer.Controls;

public class AutoUpdatingTextInput : TextInput
{
    private readonly Func<string> _updateFn;

    public bool UpdateOnlyWhenNotFocused { get; set; } = true;
    public TimeSpan UpdateFrequency { get; set; } = TimeSpan.FromSeconds(0.5);

    private TimeSpan _lastUpdate = TimeSpan.Zero;
    
    public AutoUpdatingTextInput(Func<string> updateFn)
    {
        _updateFn = updateFn;
    }
    
    protected override void OnUpdate(GameTime gameTime)
    {
        base.OnUpdate(gameTime);

        if (!Focused || !UpdateOnlyWhenNotFocused)
        {
            if (gameTime.TotalGameTime - _lastUpdate >= UpdateFrequency)
            {
                Value = _updateFn();
                _lastUpdate = gameTime.TotalGameTime;
            }
        }
    }
}