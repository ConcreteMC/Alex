using System.Collections;
using System.Collections.Generic;

namespace Alex.API.Inventory
{
    public class ContainerBase : IContainer
    {
        public ContainerStack this[int index]
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public ContainerStack this[int row, int col]
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public ContainerStack GetAt(int      index)
        {
            throw new System.NotImplementedException();
        }

        public ContainerStack SetAt(int      index, ContainerStack containable)
        {
            throw new System.NotImplementedException();
        }

        public ContainerStack AddAt(int      index, ContainerStack containable)
        {
            throw new System.NotImplementedException();
        }

        public ContainerStack SubtractAt(int index, ContainerStack containable)
        {
            throw new System.NotImplementedException();
        }


        public IEnumerator<ContainerStack> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}