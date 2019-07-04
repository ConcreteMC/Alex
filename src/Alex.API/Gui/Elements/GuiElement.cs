using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Attributes;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements
{
	public delegate bool GuiElementPredicate(IGuiElement element);

	public delegate bool GuiElementPredicate<in TGuiElement>(TGuiElement element)
		where TGuiElement : class, IGuiElement;

	public partial class GuiElement : IGuiElement
	{
		[DebuggerVisible]
		public Guid Id { get; } = Guid.NewGuid();

		private IGuiScreen       _screen;
		private IGuiElement      _parentElement;
		private IGuiFocusContext _focusContext;

		//[DebuggerVisible(Visible = false)]
		//public IGuiScreen Screen
		//{
		//	get => _screen;
		//	private set
		//	{
		//		var currentScreen = _screen;
		//		_screen = value;
		//		OnScreenChanged(currentScreen, _screen);

		//		InvalidateLayout();
		//	}
		//}

		[DebuggerVisible(Visible = false)]
		public virtual IGuiScreen Screen
		{
			get => ParentElement?.Screen;
		}

		[DebuggerVisible(Visible = false)]
		public IGuiElement ParentElement
		{
			get => _parentElement;
			set
			{
				var previousParent = _parentElement;
				_parentElement = value;
				//TryFindParentOfType<IGuiScreen>(e => true, out IGuiScreen screen);
				//Screen = screen;

				OnParentElementChanged(previousParent, _parentElement);

				InvalidateLayout();
			}
		}


		[DebuggerVisible(Visible = false)]
		public virtual IGuiFocusContext FocusContext
		{
			get { return _focusContext ?? ParentElement?.FocusContext ?? Screen; }
			set { _focusContext = value; }
		}


		[DebuggerVisible(Visible = false)]
		public IGuiElement[] ChildElements
		{
			get => Children.ToArray();
		}


		[DebuggerVisible(Visible = false)]
		protected List<IGuiElement> Children { get; } = new List<IGuiElement>();

		[DebuggerVisible(Visible = false)]
		public bool HasChildren => Children.Any();

		public int ChildCount => Children.Count;

		[DebuggerVisible(Visible = false)]
		internal IReadOnlyList<IGuiElement> AllChildren =>
			Children.OfType<GuiElement>().SelectMany(c => new[] {c}.Union(c.AllChildren)).ToList();

		#region Drawing

		[DebuggerVisible]
		public virtual Vector2 RenderPosition { get; set; }

		[DebuggerVisible]
		public virtual Size RenderSize { get; set; }

		[DebuggerVisible]
		public virtual Rectangle RenderBounds { get; set; }

		[DebuggerVisible]
		public bool IsVisible { get; set; } = true;

		public Matrix LayoutTransform { get; set; } = Matrix.Identity;
		public Matrix RenderTransform { get; set; } = Matrix.Identity;

		public void Draw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			if (!IsVisible) return;
			if (RenderBounds.Size == Point.Zero) return;

			IDisposable clipDispose = null;

			if (ClipToBounds)
				clipDispose = graphics.BeginClipBounds(RenderBounds, true);

			using (clipDispose)
			{
				if (_initialised)
				{
					OnDraw(graphics, gameTime);
				}

				ForEachChild(c => c.Draw(graphics, gameTime));
			}
		}

		#endregion

		private IGuiRenderer _guiRenderer;
		private bool         _initialised;

		protected IGuiRenderer GuiRenderer => _guiRenderer;

		public GuiElement()
		{
		}

		#region Methods

		public void Init(IGuiRenderer renderer, bool force = false)
		{
			if (!_initialised || force)
			{
				_guiRenderer = renderer;
				OnInit(renderer);
			}

			ForEachChild(c => c.Init(renderer, force));

			_initialised = true;
		}

		protected virtual void OnInit(IGuiRenderer renderer)
		{
			Background.TryResolveTexture(renderer);
			BackgroundOverlay.TryResolveTexture(renderer);
		}

		public void Update(GameTime gameTime)
		{
			if (_initialised)
			{
				OnUpdate(gameTime);
			}

			ForEachChild(c => c.Update(gameTime));
		}

		protected virtual void OnUpdate(GameTime gameTime)
		{
		}


		public void AddChild(IGuiElement element)
		{
			if (element == this) return;
			if (element.ParentElement == this) return;
			if (Children.Contains(element)) return;
			
			Children.Add(element);
			element.ParentElement = this;

			if (_initialised)
			{
				element.Init(_guiRenderer);
			}

			OnChildAdded(element);

			InvalidateLayout();
		}

		public void RemoveChild(IGuiElement element)
		{
			if (element == this) return;

			OnChildRemoved(element);

			Children.Remove(element);
			
			if(element.ParentElement == this)
				element.ParentElement = null;
			
			InvalidateLayout();
		}

		#endregion


		#region Hierachy Transcending

		public bool TryTranscendChildren(GuiElementPredicate predicate, bool recurse = true)
		{
			if (!HasChildren) return false;

			var children = Children.ToArray();

			// First scan the children at this level
			foreach (var child in children)
			{
				if (predicate(child))
				{
					return true;
				}
			}

			if (!recurse) return false;

			// If the children on this level do not match, check their children.
			foreach (var child in children)
			{
				if (child.TryTranscendChildren(predicate, true))
				{
					return true;
				}
			}

			return false;
		}

		public bool TryFindParent(GuiElementPredicate predicate, out IGuiElement parentElement)
		{
			if (ParentElement == null)
			{
				parentElement = null;
				return false;
			}

			if (predicate(ParentElement))
			{
				parentElement = ParentElement;
				return true;
			}

			return ParentElement.TryFindParent(predicate, out parentElement);
		}

		public bool TryFindParentOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate,
													 out TGuiElement                  parentElement)
			where TGuiElement : class, IGuiElement
		{
			var result = TryFindParent(e => e is TGuiElement e1 && predicate(e1), out IGuiElement element);

			parentElement = element as TGuiElement;
			return result;
		}

		public bool TryFindDeepestChild(GuiElementPredicate predicate, out IGuiElement childElement)
		{
			childElement = null;
			if (!HasChildren) return false;

			var children = Children.ToArray();

			foreach (var child in children)
			{
				if (predicate(child))
				{
					childElement = child;

					if (child.TryFindDeepestChild(predicate, out var recurseChild))
					{
						childElement = recurseChild;
						return true;
					}

					return true;
				}
			}

			// If the children on this level do not match, check their children.
			foreach (var child in children)
			{
				if (child.TryFindDeepestChild(predicate, out var recurseChild))
				{
					childElement = recurseChild;
					return true;
				}
			}

			return false;
		}

		public bool TryFindDeepestChildOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate,
														   out TGuiElement                  childElement)
			where TGuiElement : class, IGuiElement
		{
			var result = TryFindDeepestChild(e => e is TGuiElement e1 && predicate(e1), out IGuiElement element);

			childElement = element as TGuiElement;
			return result;
		}

		public IEnumerable<TResult> ForEachChild<TResult>(Func<IGuiElement, TResult> valueSelector)
		{
			if (HasChildren)
			{
				foreach (var child in Children.ToArray())
				{
					yield return valueSelector(child);
				}
			}
		}

		public void ForEachChild(Action<IGuiElement> childAction)
		{
			if (!HasChildren) return;

			foreach (var child in Children.ToArray())
			{
				childAction(child);
			}
		}

		private void ForEachChild<TElement>(Action<TElement> childAction) where TElement : class, IGuiElement
		{
			ForEachChild(c =>
			{
				if (c is TElement e) childAction(e);
			});
		}

		#endregion

		#region Event Handlers

		protected virtual void OnChildAdded(IGuiElement element)
		{
		}

		protected virtual void OnChildRemoved(IGuiElement element)
		{
		}

		protected virtual void OnScreenChanged(IGuiScreen previousScreen, IGuiScreen newScreen)
		{
		}

		protected virtual void OnParentElementChanged(IGuiElement previousParent, IGuiElement newParent)
		{
			ForEachChild(e => e.ParentElement = this);
		}

		protected virtual void OnUpdateLayout()
		{
		}

		#endregion
	}
}