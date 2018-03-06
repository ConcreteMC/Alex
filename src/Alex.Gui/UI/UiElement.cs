using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Alex.Graphics.Annotations;
using Alex.Graphics.UI.Abstractions;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Layout;
using Alex.Graphics.UI.Rendering;
using Alex.Graphics.UI.Themes;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI
{
	public class UiElement : INotifyPropertyChanged
	{
		public event EventHandler SizeChanged;
		public event EventHandler LayoutChanged;

		public UiContainer Container { get; internal set; }

		private UiRoot Root
		{
			get { return this as UiRoot ?? TranscendParentsFor(c => c as UiRoot, r => r != null); }
		}

		private UiElementLayoutParameters _layoutParameters = new UiElementLayoutParameters();

		protected internal UiElementLayoutParameters LayoutParameters
		{
			get { return _layoutParameters; }
			set
			{
				if (_layoutParameters == value) return;
				_layoutParameters = value;
				OnPropertyChanged();
			}
		}

		private bool _visible = true;

		public bool Visible
		{
			get { return _visible; }
			set
			{
				if (_visible == value) return;
				_visible = value;
				OnPropertyChanged();
			}
		}

		private string _className;

		public string ClassName
		{
			get { return _className; }
			set
			{
				if (_className == value) return;
				_className = value;
				OnPropertyChanged();
			}
		}

		private bool _isLayoutDirty = true;
		private bool _isStyleDirty = true;

		#region Styling Properties

		private UiElementStyle _style = new UiElementStyle();
		public UiElementStyle Style
		{
			get { return _style; }
			private set { _style = value; MarkStyleDirty(); }
		}

		#endregion

		public UiElement()
		{

		}
		
		public void UpdateLayout()
		{
			Root?.UpdateLayoutInternal();
		}

		internal void UpdateLayoutInternal()
		{
			UiElementLayoutParameters layoutParameters = LayoutParameters;

			layoutParameters.ContentSize = GetContentSize();

			OnUpdateLayout(layoutParameters);

			if (Container != null)
			{
				var containerSize = Container.LayoutParameters.InnerBounds.Size;

				if (layoutParameters.PositionAnchor.HasValue)
				{
					layoutParameters.Position = ((containerSize.ToVector2() * layoutParameters.PositionAnchor.Value) + (layoutParameters.OuterBounds.Size.ToVector2() * layoutParameters.PositionAnchorOrigin)).ToPoint() + Container.LayoutParameters.InnerBounds.Location;
				}


				if (layoutParameters.SizeAnchor.HasValue)
				{
					layoutParameters.Size = (containerSize.ToVector2() * layoutParameters.SizeAnchor.Value);
				}
			}

			LayoutParameters = layoutParameters;

			LayoutChanged?.Invoke(this, null);

			_isLayoutDirty = false;
		}

		protected virtual void OnUpdateLayout(UiElementLayoutParameters layoutParameters)
		{
			if (Style.PositionAnchor.HasValue)
			{
				layoutParameters.PositionAnchor = Style.PositionAnchor.Value;
			}
			else
			{
				layoutParameters.PositionAnchor = TranscendParentsFor(c => c.Style.PositionAnchor) ?? Vector2.Zero;
			}

			layoutParameters.PositionAnchorOrigin = Style.PositionAnchorOrigin ?? layoutParameters.PositionAnchor ?? Vector2.Zero;

			if (Style.SizeAnchor.HasValue)
			{
				layoutParameters.SizeAnchor = Style.SizeAnchor.Value;
			}
			layoutParameters.SizeAnchorOrigin = Style.SizeAnchorOrigin ?? layoutParameters.PositionAnchorOrigin;

			layoutParameters.Padding = Style.Padding ?? Thickness.Zero;
			layoutParameters.Margin = Style.Margin ?? Thickness.Zero;
			layoutParameters.MinSize = new Point(Style.MinWidth ?? 0, Style.MinHeight ?? 0);
		}

		protected virtual Vector2 GetContentSize()
		{
			if (this is ITextElement textElement)
			{
				var textSize = Style.TextFont?.MeasureString(textElement.Text) ?? Vector2.Zero;
				return new Vector2(textSize.X * Style.TextSize ?? 1.0f, textSize.Y * Style.TextSize ?? 1.0f);
			}

			return Vector2.Zero;
		}

		public void Update(GameTime gameTime)
		{
			if (_isLayoutDirty)
			{
				UpdateLayout();
				_isLayoutDirty = false;
			}

			OnUpdate(gameTime);
		}

		protected virtual void OnUpdate(GameTime gameTime)
		{

		}

		public void Draw(GameTime gameTime, UiRenderer renderer)
		{
			if (_isStyleDirty)
			{
				Style = renderer.Theme.GetCompiledStyleFor(this);

				MarkLayoutDirty();
				_isStyleDirty = false;
			}

			OnDraw(gameTime, renderer);
		}

		protected virtual void OnDraw(GameTime gameTime, UiRenderer renderer)
		{
			renderer.DrawElement(this);
		}

		protected void MarkStyleDirty()
		{
			if (_isStyleDirty) return;
			_isStyleDirty = true;
		}

		protected void MarkLayoutDirty()
		{
			if (_isLayoutDirty) return;
			_isLayoutDirty = true;
		}


		protected TValue TranscendParentsFor<TValue>(Func<UiContainer, TValue> func)
		{
			var d = default(TValue);
			return TranscendParentsFor(func, v => !Equals(d, v));
		}

		protected TValue TranscendParentsFor<TValue>(Func<UiContainer, TValue> func, Func<TValue, bool> condition)
		{
			if (Container != null)
			{
				var containerValue = func(Container);

				if (condition(containerValue))
				{
					return containerValue;
				}

				return Container.TranscendParentsFor<TValue>(func, condition);
			}

			return default(TValue);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			MarkStyleDirty();
		}
	}
}
