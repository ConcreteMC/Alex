namespace Alex.API.Inventory
{
    public class ContainerItemTypeMismatchException : ContainerTransactionException
    {
        protected internal ContainerItemTypeMismatchException(IContainer container, ContainerStack sourceContainerStack,
            ContainerStack targetContainerStack) : base(container, sourceContainerStack, targetContainerStack)
        {
        }
    }
}