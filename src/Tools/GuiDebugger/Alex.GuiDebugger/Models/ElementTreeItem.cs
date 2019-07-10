using System;

namespace Alex.GuiDebugger.Models
{
    public class ElementTreeItem
    {
        public Guid Id { get; set; }

        public string ElementType { get; set; }

        public ElementTreeItem[] Children { get; set; }


        public ElementTreeItem()
        {
            Children = new ElementTreeItem[0];
        }

        public ElementTreeItem(Guid id, string elementType) : this()
        {
            Id = id;
            ElementType = elementType;
        }

    }
}
