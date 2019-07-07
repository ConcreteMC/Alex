using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Alex.API.Utils
{
    public class BitArray : IEnumerable<bool>
    {
        private byte[] Data { get; set; }
        public int Length => 8 * Data.Length;

        public BitArray(byte[] values)
        {
            Data = values;
        }

        public bool this[int index]
        {
            get
            {
                int byteIndex = index / 8;
                int offset = index % 8;
                return (Data[byteIndex] & (1 << offset)) != 0;
            }
            set
            {
                int byteIndex = index / 8;
                int offset = index % 8;
                byte mask = (byte) (1 << offset);

                if (value)
                {
                    Data[byteIndex] |= mask;
                }
                else
                {
                    Data[byteIndex] = (byte)(Data[byteIndex] & ~mask);
                }
            }
        }

        public void Set(int index, bool value)
        {
            this[index] = value;
        }

        public bool Get(int index)
        {
            return this[index];
        }

        public IEnumerator<bool> GetEnumerator()
        {
            for (int bitPos = 0; bitPos < 8 * Data.Length; bitPos++)
            {
                int byteIndex = bitPos / 8;
                int offset = bitPos % 8;
                bool isSet = (Data[byteIndex] & (1 << offset)) != 0;

                yield return isSet;

                bitPos++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
