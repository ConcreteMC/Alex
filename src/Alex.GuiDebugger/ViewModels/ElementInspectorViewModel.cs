using System.Linq;
using Alex.GuiDebugger.Models;
using Alex.GuiDebugger.Services;
using Catel.Fody;
using Orc.SelectionManagement;

namespace Alex.GuiDebugger.ViewModels
{
    using Catel.MVVM;

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class ElementInspectorViewModel : ViewModelBase
    {
        private readonly IGuiDebugDataService _guiDebugDataService;
        private readonly ISelectionManager<GuiDebuggerElementInfo> _guiDebuggerElementInfoSelectionManager;

        [Model]
        [Expose(nameof(GuiDebuggerElementInfo.Properties))]
        public GuiDebuggerElementInfo ElementInfo { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ElementInspectorViewModel"/> class.
        /// </summary>
        public ElementInspectorViewModel(IGuiDebugDataService guiDebugDataService, ISelectionManager<GuiDebuggerElementInfo> guiDebuggerElementInfoSelectionManager)
        {
            _guiDebugDataService = guiDebugDataService;
            _guiDebuggerElementInfoSelectionManager = guiDebuggerElementInfoSelectionManager;
            _guiDebuggerElementInfoSelectionManager.SelectionChanged += GuiDebuggerElementInfoSelectionManagerOnSelectionChanged;

            Title = "Element Inspector";

            ElementInfo = _guiDebuggerElementInfoSelectionManager.GetSelectedItem();
        }

        private void GuiDebuggerElementInfoSelectionManagerOnSelectionChanged(object sender, SelectionChangedEventArgs<GuiDebuggerElementInfo> e)
        {
            ElementInfo = _guiDebuggerElementInfoSelectionManager.GetSelectedItem();

            if (ElementInfo != null)
            {
                _guiDebugDataService.RefreshProperties(ElementInfo);
            }
        }
    }
}
