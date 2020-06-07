using System;

namespace Alex.Blocks.Storage
{
    public interface IStorage
    {
        uint this[int index] { get; set; }

        int Length { get; }
    }
}