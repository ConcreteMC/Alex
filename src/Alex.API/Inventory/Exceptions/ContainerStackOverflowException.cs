namespace Alex.API.Inventory
{
    public class ContainerStackOverflowException : ContainerTransactionException
    {
        protected internal ContainerStackOverflowException(IContainer container, ContainerStack sourceContainerStack,
            ContainerStack targetContainerStack) : base(container, sourceContainerStack, targetContainerStack)
        {
        }
    }
}