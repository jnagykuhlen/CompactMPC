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
            : base(numberOfElements, ElementsPerByte) { }

        public QuadrupleIndexArray(int[] elements)
            : base(elements, ElementsPerByte) { }

        protected QuadrupleIndexArray(byte[] bytes, int numberOfElements, int elementsPerByte)
            : base(bytes, numberOfElements, elementsPerByte) { }

        public static QuadrupleIndexArray FromBytes(byte[] bytes, int numberOfElements)
        {
            return new QuadrupleIndexArray(bytes, numberOfElements, ElementsPerByte);
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
            WriteBits((byte)value, index, ElementsPerByte, BitMask);
        }
    }
}
