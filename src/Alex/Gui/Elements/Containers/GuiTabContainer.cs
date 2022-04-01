using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Attributes;

namespace Alex.Gui.Elements.Containers;

public class GuiTabContainer : RocketElement
{
    private readonly Container _rootContainer;
    private readonly Container _contentContainer;
    private readonly StackContainer _tabItemContainer;
    
    private TabPosition _tabPositioning = TabPosition.Bottom;
    private GuiTab _activeTab = null;

    private ObservableCollection<GuiTab> _tabs;
    public GuiTabContainer()
    {
        _tabs = new ObservableCollection<GuiTab>();
        _tabs.CollectionChanged += TabsOnCollectionChanged;
        
        _rootContainer = new Container()
        {
            Anchor = Alignment.Fill
        };
        _tabItemContainer = new StackContainer()
        {
            Background = Color.Red
        };
        _contentContainer = new Container()
        {
            Anchor = Alignment.Fill,
            Background = Color.White
        };
        
        AddChild(_rootContainer);
    }

    public TabPosition TabPositioning
    {
        get => _tabPositioning;
        set
        {
            _tabPositioning = value;
            UpdatePositioning();
        }
    }

    public GuiTab ActiveTabControl
    {
        get => _activeTab;
        set
        {
            var oldTabControl = _activeTab;
            _activeTab = value;
            if (oldTabControl != null)
            {
                _contentContainer.RemoveChild(oldTabControl);
            }

            if (value != null)
            {
                _contentContainer.AddChild(value);
            }
        }
    }

    public ICollection<GuiTab> Tabs => _tabs;

    private void TabsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        var tabContainer = _tabItemContainer;
        
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (GuiTab tab in e.NewItems)
            {
                tabContainer.AddChild(tab.TabButton);
            }

            if (_activeTab == null)
                ActiveTabControl = _tabs.FirstOrDefault();
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (GuiTab tab in e.OldItems)
            {
                tabContainer.RemoveChild(tab.TabButton);
                
                if (_activeTab == tab)
                    ActiveTabControl = _tabs.FirstOrDefault();
            }
        }
    }

    private void UpdatePositioning()
    {
        var rootContainer = _rootContainer;
        var contentContainer = _contentContainer;
        var tabContainer = _tabItemContainer;
        
        var positioning = _tabPositioning;
        switch (positioning)
        {
            case TabPosition.Top:
                //rootContainer.Orientation = Orientation.Vertical;
                tabContainer.Orientation = Orientation.Horizontal;
                tabContainer.Anchor = Alignment.TopFill;
                tabContainer.ChildAnchor = Alignment.TopLeft;
                
                rootContainer.RemoveChild(contentContainer);
                rootContainer.RemoveChild(tabContainer);
                
                rootContainer.AddChild(tabContainer);
                rootContainer.AddChild(contentContainer);
                break;
            case TabPosition.Bottom:
                //rootContainer.Orientation = Orientation.Vertical;
                tabContainer.Orientation = Orientation.Horizontal;
                tabContainer.Anchor = Alignment.BottomFill;
                tabContainer.ChildAnchor = Alignment.BottomLeft;
                
                rootContainer.RemoveChild(contentContainer);
                rootContainer.RemoveChild(tabContainer);
                
                rootContainer.AddChild(contentContainer);
                rootContainer.AddChild(tabContainer);
                break;
            case TabPosition.Left:
                //rootContainer.Orientation = Orientation.Horizontal;
                tabContainer.Orientation = Orientation.Vertical;
                tabContainer.Anchor = Alignment.FillLeft;
                tabContainer.ChildAnchor = Alignment.TopLeft;
                rootContainer.RemoveChild(contentContainer);
                rootContainer.RemoveChild(tabContainer);
                
                rootContainer.AddChild(tabContainer);
                rootContainer.AddChild(contentContainer);
                break;
            case TabPosition.Right:
                //rootContainer.Orientation = Orientation.Horizontal;
                tabContainer.Orientation = Orientation.Vertical;
                tabContainer.Anchor = Alignment.FillRight;
                tabContainer.ChildAnchor = Alignment.TopRight;
                rootContainer.RemoveChild(contentContainer);
                rootContainer.RemoveChild(tabContainer);
                
                rootContainer.AddChild(contentContainer);
                rootContainer.AddChild(tabContainer);
                break;
        }
    }
    
    protected override void OnInit(IGuiRenderer renderer)
    {
        base.OnInit(renderer);
        
        UpdatePositioning();
    }

    protected override void OnAfterMeasure()
    {
        base.OnAfterMeasure();
        
        var size = Size;
        
        var contentContainer = _contentContainer;
        var tabContainer = _tabItemContainer;
        var positioning = _tabPositioning;
        switch (positioning)
        {
            case TabPosition.Bottom:
            case TabPosition.Top:
                var contentHeight = (int)Math.Ceiling(size.Height * 0.95);
                var remainingHeight = size.Height - contentHeight;
                
                contentContainer.Height = contentContainer.MinHeight = contentHeight;
                contentContainer.Width = contentContainer.MinWidth = size.Width;

                tabContainer.Height = tabContainer.MinHeight = remainingHeight;
                tabContainer.Width = tabContainer.MinWidth = size.Width;
                break;
            case TabPosition.Left:
            case TabPosition.Right:
                var contentWidth = (int)Math.Ceiling(size.Width * 0.95);
                var remainingWidth = size.Width - contentWidth;
                
                contentContainer.Height = contentContainer.MinHeight = size.Height;
                contentContainer.Width = contentContainer.MinWidth = contentWidth;

                tabContainer.Height = tabContainer.MinHeight = size.Height;
                tabContainer.Width = tabContainer.MinWidth = remainingWidth;
                break;
        }
    }

    protected override void OnChildAdded(IGuiElement element)
    {
        base.OnChildAdded(element);
    }

    protected override void OnChildRemoved(IGuiElement element)
    {
        base.OnChildRemoved(element);
    }
}

public class GuiTab : Container
{
    [DebuggerVisible]
    public string Label
    {
        get => TabButton.Text;
        set => TabButton.Text = value;
    }

    internal Button TabButton { get; private set; }

    public GuiTab()
    {
        TabButton = new Button();
        TabButton.Action = () =>
        {
            if (TryFindParent(e => e is GuiTabContainer, out var parent) && parent is GuiTabContainer tabContainer)
            {
                tabContainer.ActiveTabControl = this;
            }
        };
    }
}

public enum TabPosition
{
    Top,
    Bottom,
    Left,
    Right
}