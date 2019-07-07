using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public class PairIndexArray : PackedArray<int>
    {
        private const int ElementsPerByte = 8;
        private const int BitMask = 0b1;

        public PairIndexArray(int numberOfElements)
            : base(RequiredBytes(numberOfElements), numberOfElements) { }

        public PairIndexArray(int[] elements)
            : base(RequiredBytes(elements.Length), elements) { }

        protected PairIndexArray(byte[] bytes, int numberOfElements)
            : base(bytes, RequiredBytes(numberOfElements), numberOfElements) { }

        public static PairIndexArray FromBytes(byte[] bytes, int numberOfElements)
        {
            return new PairIndexArray(bytes, numberOfElements);
        }

        public static int RequiredBytes(int numberOfElements)
        {
            return RequiredBytes(numberOfElements, ElementsPerByte);
        }

        protected override int ReadElement(int index)
        {
            return (int)ReadBits(index, ElementsPerByte, BitMask);
        }

        protected override void WriteElement(int value, int index)
        {
            if (value < 0 || value >= 2)
                throw new ArgumentOutOfRangeException(nameof(value), "Pair index must be in the range from 0 to 1.");

            WriteBits((byte)value, index, ElementsPerByte, BitMask);
        }

        public PairIndexArray Clone()
        {
            return new PairIndexArray(Buffer, Length);
        }
    }
}
