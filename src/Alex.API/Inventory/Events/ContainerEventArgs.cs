using System;

namespace Alex.API.Inventory
{
    public class ContainerEventArgs : EventArgs
    {
        public IContainer Container { get; }

        protected internal ContainerEventArgs(IContainer container)
        {
            Container = container;
        }
    }
}