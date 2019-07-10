using System;
using System.Collections.Generic;
using System.Text;
using Alex.GuiDebugger.Common.Services;
using Alex.GuiDebugger.Models;
using Catel.Caching;
using Catel.Threading;
using Orc.SelectionManagement;

namespace Alex.GuiDebugger.Services
{
    internal class PollingService
    {
        private IGuiDebuggerService DebuggerService { get; }
        public ISelectionManager<GuiDebuggerElementInfo> SelectionManager { get; }
        public ICacheStorage<Guid, GuiDebuggerElementInfo> ElementCache { get; }
        private Timer PollingTimer { get; }

        public PollingService(IGuiDebuggerService debugger, ISelectionManager<GuiDebuggerElementInfo> selectionManager, ICacheStorage<Guid, GuiDebuggerElementInfo> elementCache)
        {
            DebuggerService = debugger;
            SelectionManager = selectionManager;
            ElementCache = elementCache;

            PollingTimer = new Timer(Callback, null, 0, 500);
        }

        private void Callback(object state)
        {
            var result = DebuggerService.TryGetElementUnderCursor();
            if (!result.HasValue)
                return;

            var value = result.Value;
            var selectedItem = SelectionManager.GetSelectedItem();
            if (selectedItem == null || selectedItem.Id.Equals(value))
                return;

            selectedItem = ElementCache.Get(value);
            if (selectedItem == null)
                return;

            SelectionManager.Replace(selectedItem);
        }
    }
}
