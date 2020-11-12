using System;

namespace Alex.Worlds.Chunks
{
    public static class ArrayOf<T> where T : new()
    {
        public static T[] Create(int size, T initialValue)
        {
            T[] array = (T[])Array.CreateInstance(typeof(T), size);
            for (int i = 0; i < array.Length; i++)
                array[i] = initialValue;
            return array;
        }

        public static T[] Create(int size)
        {
            T[] array = (T[])Array.CreateInstance(typeof(T), size);
            for (int i = 0; i < array.Length; i++)
                array[i] = new T();
            return array;
        }
    }
}