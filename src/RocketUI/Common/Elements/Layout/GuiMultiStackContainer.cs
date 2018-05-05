using System;
using System.Collections.Generic;

namespace RocketUI.Elements.Layout
{
    public class GuiMultiStackContainer : GuiStackContainer
    {
        private List<GuiStackContainer> _rows = new List<GuiStackContainer>();

        private readonly Action<GuiStackContainer> _defaultRowBuilder;

        public GuiMultiStackContainer(Action<GuiStackContainer> defaultRowBuilder = null)
        {
            _defaultRowBuilder = defaultRowBuilder;
        }

        public void AddChild(int row, IVisualElement element)
        {
            EnsureRows(row + 1);

            _rows[row].AddChild(element);
        }

        private void EnsureRows(int count)
        {
            if (_rows.Count < count)
            {
                for (int i = 0; i < (count - _rows.Count); i++)
                {
                    AddRow();
                }
            }
        }

        public GuiStackContainer AddRow(Action<GuiStackContainer> rowBuilder = null)
        {
            var stack = new GuiStackContainer()
            {
                Orientation = Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal,
                ChildAnchor = ChildAnchor.SwapXY()
            };

            _defaultRowBuilder?.Invoke(stack);
            rowBuilder?.Invoke(stack);

            _rows.Add(stack);
            AddChild(stack);
            return stack;
        }
    }
}
