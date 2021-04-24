namespace Alex.API.Inventory
{
    public class ContainerStack
    {
        public static ushort       DefaultMaxStackSize { get; set; } = 64;
        public        IContainable Item                { get; set; }
        public        ushort       Count               { get; set; }

        public int MaxStackSize
        {
            get => Item?.MaxStackSize ?? DefaultMaxStackSize;
        }

        protected internal IContainer Container { get; set; }


        public ContainerStack()
        {
        }
        
        public ContainerStack(IContainable item, ushort count = 1)
        {
            Item = item;
            Count = count;
        }

        public static ContainerStack operator +(ContainerStack a, ContainerStack b)
        {
            if (a.Item != b.Item) throw new ContainerItemTypeMismatchException(b.Container, a, b);

            int count = b.Count;
            if (count + a.Count > a.MaxStackSize) throw new ContainerStackOverflowException(a.Container, b, a);

            a.Count += (ushort)count;

            return a;
        }

        public static ContainerStack operator -(ContainerStack a, ContainerStack b)
        {
            if (a.Item != b.Item) throw new ContainerItemTypeMismatchException(a.Container, b, a);

            int count = b.Count;
            if (a.Count - count < 0) throw new ContainerStackOverflowException(a.Container, b, a);

            a.Count -= (ushort)count;
            return a;
        }

        public static ContainerStack operator +(ContainerStack a, ushort num)
        {
            int newCount = a.Count + num;
            
            if (newCount > a.MaxStackSize) throw new ContainerStackOverflowException(a.Container, new ContainerStack(a.Item, num), a);
            if (newCount < 0) throw new ContainerStackOverflowException(a.Container, new ContainerStack(a.Item, num), a);

            a.Count = (ushort) newCount;
            return a;
        }

        public static ContainerStack operator -(ContainerStack a, ushort num)
        {
            int newCount = a.Count - num;
            
            if (newCount > a.MaxStackSize) throw new ContainerStackOverflowException(a.Container, new ContainerStack(a.Item, num), a);
            if (newCount < 0) throw new ContainerStackOverflowException(a.Container, new ContainerStack(a.Item, num), a);

            a.Count = (ushort) newCount;
            return a;
        }
    }
}