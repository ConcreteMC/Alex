using System;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiToggleButton : GuiButton, IValuedControl<bool>
    {

	    public event EventHandler<bool> ValueChanged;
	    private bool _value;

		public GuiTexture2D CheckedBackground;

		public virtual Color CheckedOutlineColor { get; set; } = new Color(Color.White, 0.75f);
		public virtual Thickness CheckedOutlineThickness { get; set; } = Thickness.Zero;

		public bool Checked
		{
			get => Value;
			set => Value = value;
		}

		public bool Value
        {
            get => _value;
            set
            {
	            if (value != _value)
	            {
		            _value = value;
		            ValueChanged?.Invoke(this, _value);
					OnCheckedChanged();
				}
			}
        }

	    public string DisplayFormat { get; set; }

	    public GuiToggleButton() : base()
	    {

	    }

		public GuiToggleButton(string text) : base(text)
		{

		}
		protected virtual void OnCheckedChanged()
		{
			if (Checked)
			{
				if (Modern)
				{
					TextElement.TextColor = TextColor.Cyan;
				}
				else
				{
					TextElement.TextColor = TextColor.Yellow;
				}
			}
			else
			{
				if (Modern)
				{
					OnEnabledChanged();
				}
				else
				{
					TextElement.TextColor = TextColor.White;
				}
			}
		}

		protected override void OnHighlightDeactivate()
		{
			base.OnHighlightDeactivate();

			if (Checked)
			{
				OnCheckedChanged();
			}
		}

		protected override void OnFocusDeactivate()
		{
			base.OnFocusDeactivate();

			if (Checked)
			{
				OnCheckedChanged();
			}
		}

		protected override void OnCursorPressed(Point cursorPosition)
	    {
		    Value = !_value;
	    }

		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
			CheckedBackground.TryResolveTexture(renderer);
		}

		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			base.OnDraw(graphics, gameTime);

			if (Enabled)
			{
				if (Value)
				{
					graphics.FillRectangle(RenderBounds, CheckedBackground);

					if (CheckedOutlineThickness != Thickness.Zero)
					{
						graphics.DrawRectangle(RenderBounds, FocusOutlineColor, FocusOutlineThickness, true);
					}
				}
			}
		}
	}
}
