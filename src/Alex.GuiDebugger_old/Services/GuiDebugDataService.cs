using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Common.Services;
using Alex.GuiDebugger.Models;
using Catel.Caching;
using Catel.Collections;
using Catel.IoC;

namespace Alex.GuiDebugger.Services
{
	public class GuiDebugDataService : IGuiDebugDataService
	{
		private readonly IGuiDebuggerService _guiDebuggerService;
		private readonly ICacheStorage<Guid, GuiDebuggerElementInfo> _guiDebuggerElementInfoCache;
		public           GuiDebuggerData     GuiDebuggerData { get; }

		public GuiDebugDataService(IGuiDebuggerService guiDebuggerService, ICacheStorage<Guid, GuiDebuggerElementInfo> guiDebuggerElementInfoCache)
		{
			_guiDebuggerService = guiDebuggerService;
			_guiDebuggerElementInfoCache = guiDebuggerElementInfoCache;

			GuiDebuggerData = new GuiDebuggerData();
		}
		
		public void RefreshElements()
		{
			var allElementInfos = _guiDebuggerService.GetAllGuiElementInfos();

			var elements = GuiDebuggerData.Elements;

			var newItems = allElementInfos.Select(Convert).ToArray();

			//using (elements.SuspendChangeNotifications(SuspensionMode.MixedConsolidate))
			{
				elements.Clear();
				elements.AddItems(newItems);
			}
		}

		public void RefreshProperties(GuiDebuggerElementInfo elementInfo)
		{
			var propertyInfos = _guiDebuggerService.GetElementPropertyInfos(elementInfo.Id);

			foreach (var existingElementProp in elementInfo.Properties.ToArray())
			{
				if (propertyInfos.All(x => x.Name != existingElementProp.Name))
				{
					elementInfo.Properties.Remove(existingElementProp);
				}
			}

			foreach (var property in propertyInfos)
			{
				var elementProp = elementInfo.Properties.FirstOrDefault(x => x.Name == property.Name);
				if (elementProp == null)
				{
					elementProp = new GuiDebuggerElementPropertyInfo()
					{
						Name = property.Name
					};

					elementInfo.Properties.Add(elementProp);
				}

				elementProp.Value = property.Value;
			}

		}

		private GuiDebuggerElementInfo Convert(GuiElementInfo guiElementInfo)
		{
			var model = _guiDebuggerElementInfoCache.GetFromCacheOrFetch(guiElementInfo.Id, () => new GuiDebuggerElementInfo()
			{
				Id          = guiElementInfo.Id
			});

			model.ElementType = guiElementInfo.ElementType;

			if (guiElementInfo.ChildElements != null && guiElementInfo.ChildElements.Any())
			{
				var newChildElements = guiElementInfo.ChildElements.Select(Convert).ToArray();

				//using (model.ChildElements.SuspendChangeNotifications(SuspensionMode.MixedConsolidate))
				{
					model.ChildElements.Clear();
					model.ChildElements.AddItems(newChildElements);
				}
			}

			return model;
		}


	}
}