using System;
using System.Collections.Generic;
using System.Text;
using Dock.Model;
using ReactiveUI;

namespace Alex.GuiDebugger.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private IDockFactory _factory;
        private IView        _layout;
        private string       _currentView;

        public IDockFactory Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        public IView Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public string CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }
    }
}
