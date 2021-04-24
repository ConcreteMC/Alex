namespace Alex.API.Inventory
{
    public class ContainerTransactionException : ContainerException
    {
        public ContainerStack SourceContainerStack { get; }
        public ContainerStack TargetContainerStack { get; }

        protected internal ContainerTransactionException(IContainer container, ContainerStack sourceContainerStack,
            ContainerStack                                          targetContainerStack) : base(container)
        {
            SourceContainerStack = sourceContainerStack;
            TargetContainerStack = targetContainerStack;
        }
    }
}