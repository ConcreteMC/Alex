using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Common.Services;
using Alex.GuiDebugger.Models;
using Catel.IoC;
using Catel.Logging;
using Orc.SelectionManagement;

namespace Alex.GuiDebugger.Views
{
    using Catel.Windows.Controls;

    /// <summary>
    /// Interaction logic for ElementTreeView.xaml.
    /// </summary>
    public partial class ElementTreeView : UserControl
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly ISelectionManager<GuiDebuggerElementInfo> _guiElementInfoSelectionManager;
        private readonly IGuiDebuggerService _debuggerService;
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementTreeView"/> class.
        /// </summary>
        public ElementTreeView()
        {
            InitializeComponent();

            var serviceLocator = this.GetServiceLocator();

            _guiElementInfoSelectionManager = serviceLocator.ResolveType<ISelectionManager<GuiDebuggerElementInfo>>();
            _debuggerService = serviceLocator.ResolveType<IGuiDebuggerService>();
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var treeView = sender as TreeView;
            var element = treeView?.SelectedItem as GuiDebuggerElementInfo;

            _guiElementInfoSelectionManager.Replace(element);
        }
    }
}
