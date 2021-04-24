namespace Alex.API.Inventory
{
    public class ContainerTransactionEventArgs : ContainerEventArgs
    {
        public ushort         SlotIndex              { get; set; }
        public ContainerStack PreviousContainerStack { get; set; }
        public ContainerStack NewContainerStack      { get; set; }
        
        protected internal ContainerTransactionEventArgs(IContainer container, ushort slotIndex, ContainerStack previousContainerStack, ContainerStack newContainerStack) : base(container)
        {
            SlotIndex = slotIndex;
            PreviousContainerStack = previousContainerStack;
            NewContainerStack = newContainerStack;
        }
    }
}