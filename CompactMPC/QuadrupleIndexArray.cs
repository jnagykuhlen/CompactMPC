using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public class QuadrupleIndexArray : PackedArray<int>
    {
        private const int ElementsPerByte = 4;
        private const int BitMask = 0x3;

        public QuadrupleIndexArray(int numberOfElements)
            : base(RequiredBytes(numberOfElements), numberOfElements) { }

        public QuadrupleIndexArray(int[] elements)
            : base(RequiredBytes(elements.Length), elements) { }

        protected QuadrupleIndexArray(byte[] bytes, int numberOfElements)
            : base(bytes, RequiredBytes(numberOfElements), numberOfElements) { }

        public static QuadrupleIndexArray FromBytes(byte[] bytes, int numberOfElements)
        {
            return new QuadrupleIndexArray(bytes, numberOfElements);
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
            if (value < 0 || value >= 4)
                throw new ArgumentOutOfRangeException(nameof(value), "Quadruple index must be in the range from 0 to 3.");

            WriteBits((byte)value, index, ElementsPerByte, BitMask);
        }
    }
}
