using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Alex.GuiDebugger.Views;
using Orchestra.Services;

namespace Alex.GuiDebugger.Services
{
    public class RibbonService : IRibbonService
    {
        public FrameworkElement GetRibbon()
        {
            return new RibbonView();
        }

        public FrameworkElement GetMainView()
        {
            return new MainView();
        }

        public FrameworkElement GetStatusBar()
        {
            return new StatusBarView();
        }
    }
}
