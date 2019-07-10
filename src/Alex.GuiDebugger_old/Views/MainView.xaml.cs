using Alex.GuiDebugger.Common.Services;
using Catel.IoC;

namespace Alex.GuiDebugger.Views
{
    using Catel.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainView.xaml.
    /// </summary>
    public partial class MainView : UserControl
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MainView"/> class.
        /// </summary>
        public MainView()
        {
            InitializeComponent();
            var serviceLocator = this.GetServiceLocator();
            var debuggerService = serviceLocator.ResolveType<IGuiDebuggerService>();
            debuggerService.EnableUIDebugging();
        }

    }
}
