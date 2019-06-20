using System;
using System.Collections.Generic;
using System.Text;
using Alex.GuiDebugger.Common;

namespace Alex.GuiDebugger.Models
{

	public class PropertyGridItemValueChangedEventArgs : EventArgs 
	{
		public Guid ElementId { get; }
		public string PropertyName { get; }
		public string OldValue { get; }
		public string NewValue { get; }


		internal PropertyGridItemValueChangedEventArgs(Guid id, string name, string oldValue, string newValue)
		{
			ElementId = id;
			PropertyName = name;
			OldValue = oldValue;
			NewValue = newValue;
		}
	}
	public class PropertyGridItem : GuiElementPropertyInfo
	{
		private readonly Guid _elementId;
		public static event EventHandler<PropertyGridItemValueChangedEventArgs> ValueChanged; 

		private string _stringValue;

		public override string StringValue
		{
			get => _stringValue;
			set
			{
				var oldValue = _stringValue;
				_stringValue = value;
				ValueChanged?.Invoke(this, new PropertyGridItemValueChangedEventArgs(_elementId, Name, oldValue, _stringValue));
			}
		}

		public PropertyGridItem(Guid elementId, string name, string value)
		{
			_elementId = elementId;
			Name = name;
			_stringValue = value;
		}
	}
}
