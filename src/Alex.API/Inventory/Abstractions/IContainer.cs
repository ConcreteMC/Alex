using System;
using System.Collections.Generic;

namespace Alex.API.Inventory
{
    public interface IContainer : IEnumerable<ContainerStack>
    {
        ContainerStack this[int index] { get; set; }

        ContainerStack this[int row, int col] { get; set; }

        ContainerStack GetAt(int index);
        ContainerStack SetAt(int index, ContainerStack containable);

        ContainerStack AddAt(int      index, ContainerStack containable);
        ContainerStack SubtractAt(int index, ContainerStack containable);


        public static IContainer operator +(IContainer a, ContainerStack b)
        {
            throw new NotImplementedException();
        }

        public static IContainer operator -(IContainer a, ContainerStack b)
        {
            throw new NotImplementedException();
        }
    }
}