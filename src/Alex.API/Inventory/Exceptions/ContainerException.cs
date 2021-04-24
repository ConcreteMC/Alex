using System;

namespace Alex.API.Inventory
{
    public class ContainerException : Exception
    {
        public IContainer Container { get; }

        protected internal ContainerException(IContainer container)
        {
            Container = container;
        }
    }
}