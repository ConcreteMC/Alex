using System.Threading.Tasks;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Models;
using Alex.GuiDebugger.Services;
using Catel.Collections;
using Catel.Fody;

namespace Alex.GuiDebugger.ViewModels
{
    using Catel.MVVM;

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class ElementTreeViewModel : ViewModelBase
    {
        private readonly IGuiDebugDataService _guiDebugDataService;

        [Model]
        [Expose(nameof(Alex.GuiDebugger.Models.GuiDebuggerData.Elements))]
        public GuiDebuggerData GuiDebuggerData { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementTreeViewModel"/> class.
        /// </summary>
        public ElementTreeViewModel(IGuiDebugDataService guiDebugDataService)
        {
            _guiDebugDataService = guiDebugDataService;
            Title = "Element Tree";

            GuiDebuggerData = _guiDebugDataService.GuiDebuggerData;
        }
    }
}
