using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alex.API.Gui;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Common.Services;
using NLog;
using RocketUI;

namespace Alex.Services
{
	public class GuiDebuggerService : IGuiDebuggerService
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();


		protected GuiManager GuiManager => Alex.Instance.GuiManager;

        public Guid? TryGetElementUnderCursor()
        {
            return Alex.Instance.GuiDebugHelper.TopMostHighlighted?.Id;
        }

		public void HighlightGuiElement(Guid id)
		{
			Log.Info($"IGuiDebuggerService.HighlightGuiElement(id: {id.ToString()})");
			var element = FindGuiElementById(id);
			Alex.Instance.GuiDebugHelper.HighlightedElement = element;
		}

		public void DisableHighlight()
		{
			Log.Info("IGuiDebuggerService.DisableHighlight()");
			Alex.Instance.GuiDebugHelper.HighlightedElement = null;
		}

		public GuiElementInfo[] GetAllGuiElementInfos()
		{
			Log.Info("IGuiDebuggerService.GetAllGuiElementInfos()");
			var screens = GuiManager.Screens.ToArray();
			var res = new List<GuiElementInfo>();
			
			foreach (var screen in screens)
			{
				res.Add(BuildGuiElementInfo(screen));
			}

			return res.ToArray();
		}

		public GuiElementPropertyInfo[] GetElementPropertyInfos(Guid id)
		{
			Log.Info($"IGuiDebuggerService.GetElementPropertyInfos(id: {id.ToString()})");
			var element = FindGuiElementById(id);
			if(element == null) return new GuiElementPropertyInfo[0];

			var infos = BuildGuiElementPropertyInfos(element);
			return infos;
		}

		public bool SetElementPropertyValue(Guid id, string propertyName, string propertyValue)
		{
			Log.Info($"IGuiDebuggerService.SetElementPropertyValue(id: {id.ToString()}, propertyName: {propertyName}, propertyValue: {propertyValue})");
			var element = FindGuiElementById(id);
			if (element == null) return false;

			var property = element.GetType().GetProperty(propertyName);
			if (property == null) return false;

			try
			{
				var propType = property.PropertyType;
				var value    = ConvertPropertyType(propType, propertyValue);
				property.SetValue(element, value);
				return true;
			}
			catch
			{
				return false;
			}
		}

        public void EnableUIDebugging()
        {
            Alex.Instance.GuiDebugHelper.Enabled = true;
        }

        public bool IsUIDebuggingEnabled()
        {
            return Alex.Instance.GuiDebugHelper.Enabled;
        }

        private IGuiElement FindGuiElementById(Guid id)
		{
			foreach (var screen in GuiManager.Screens.ToArray())
			{
				if (screen.TryFindDeepestChild(e => e.Id.Equals(id), out IGuiElement foundElement))
				{
                    return foundElement;
				}
			}

			return null;
		}

		private object ConvertPropertyType(Type targetType, string value)
		{
			if (targetType.IsEnum)
			{
				return Enum.Parse(targetType, value, true);
			}

			if (targetType == typeof(Size))
			{
				return Size.Parse(value);
			}

			if (targetType == typeof(Thickness))
			{
				return Thickness.Parse(value);
			}

			if (targetType == typeof(int))
			{
				return int.Parse(value);
			}
			
			if (targetType == typeof(double))
			{
				return double.Parse(value);
			}
			
			if (targetType == typeof(float))
			{
				return float.Parse(value);
			}
			
			if (targetType == typeof(bool))
			{
				return bool.Parse(value);
			}

			return Convert.ChangeType(value, targetType);
		}

		private GuiElementInfo BuildGuiElementInfo(IGuiElement guiElement)
		{
			var info = new GuiElementInfo(guiElement.Id, guiElement.GetType().Name);

			info.ChildElements = guiElement.ChildElements.ToArray().Select(BuildGuiElementInfo).ToArray();
			return info;
		}

		private GuiElementPropertyInfo[] BuildGuiElementPropertyInfos(IGuiElement guiElement)
		{
			var properties = guiElement.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

			var infos = new List<GuiElementPropertyInfo>();
			foreach (var prop in properties)
			{
				var attr = prop.GetCustomAttribute<DebuggerVisibleAttribute>(true);
				if (attr == null) continue;
				if (!attr.Visible) continue;

				if(typeof(IGuiElement).IsAssignableFrom(prop.PropertyType)) continue;

				object val = null;
				try
				{
					val = prop.GetValue(guiElement);
					
				}
				catch(Exception ex)
				{
					val = "Exception - " + ex.Message;
				}

				//infos.Add(new GuiElementPropertyInfo()
				//{
				//	Name        = prop.Name,
				//	Type        = prop.PropertyType,
				//	Value       = val,
				//	StringValue = val?.ToString()
				//});

				var propType = prop.PropertyType;


				if (propType.Assembly != typeof(IGuiDebuggerService).Assembly)
				{
					propType = typeof(string);
					val = val?.ToString();
				}

				infos.Add(new GuiElementPropertyInfo()
				{
					Name        = prop.Name,
					Type        = propType,
					Value       = val,
					StringValue = val?.ToString()
				});
			}

			return infos.ToArray();
		}
	}
}
