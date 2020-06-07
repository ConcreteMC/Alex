using Alex.API.Input;
using Alex.Items;

namespace Alex.Gui.Dialogs
{
    public interface IContainerActionHandler
    {
        void ItemClicked(ContainerItem item, MouseButton button);
        void ItemHoveredOver(ContainerItem item, MouseButton button);
        void ItemDraggedOverSlot(ContainerItem cursorItem, ContainerItem slot, MouseButton button);
        
        void HandleContainerAction(ContainerActionType action, ContainerItem cursorItem)
        {
            
        }
    }

    public class ContainerItem
    {
        public Item Item { get; set; }
        public int SlotId { get; set; }

        public ContainerItem(Item item, int slotId)
        {
            Item = item;
            SlotId = slotId;
        }
    }

    public enum ContainerActionType
    {
        LeftMouseClick,
        RightMouseClick,
        ShiftLeftMouseClick,
        ShiftRightMouseClick,
        MiddleClick,
        DropItem,
        DropFullStack,
        StartLeftMouseDrag,
        StartRightMouseDrag,
        StartMiddleMouseDrag,
        AddSlotToLeftMouseDrag,
        AddSlotToRightMouseDrag,
        AddSlotToMiddleMouseDrag,
        EndLeftMouseDrag,
        EndRightMouseDrag,
        EndMiddleMouseDrag,
        DoubleClick
    }
}