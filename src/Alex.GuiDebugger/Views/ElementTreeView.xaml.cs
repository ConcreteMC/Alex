using System.Windows;
using System.Windows.Controls;
using Alex.GuiDebugger.Common;
using Catel.IoC;
using Orc.SelectionManagement;

namespace Alex.GuiDebugger.Views
{
    using Catel.Windows.Controls;

    /// <summary>
    /// Interaction logic for ElementTreeView.xaml.
    /// </summary>
    public partial class ElementTreeView : UserControl
    {

        private readonly ISelectionManager<GuiElementInfo> _guiElementInfoSelectionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementTreeView"/> class.
        /// </summary>
        public ElementTreeView()
        {
            InitializeComponent();

            var serviceLocator = this.GetServiceLocator();

            _guiElementInfoSelectionManager = serviceLocator.ResolveType<ISelectionManager<GuiElementInfo>>();
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var treeView = sender as TreeView;
            _guiElementInfoSelectionManager.Replace(treeView?.SelectedItem as GuiElementInfo);
        }
    }
}
