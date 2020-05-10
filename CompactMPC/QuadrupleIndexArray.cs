using System;
using System.Collections.Generic;

namespace CompactMPC
{
    public class QuadrupleIndexArray : PackedArray<int>
    {
        private const int ElementsPerByte = 4;
        private const int BitMask = 0x3;

        public QuadrupleIndexArray(int numberOfElements)
            : base(numberOfElements, ElementsPerByte) { }

        public QuadrupleIndexArray(IReadOnlyList<int> elements)
            : base(elements.Count, ElementsPerByte)
        {
            for (int i = 0; i < elements.Count; ++i)
                WriteElement(elements[i], i);
        }

        private QuadrupleIndexArray(byte[] bytes, int numberOfElements, int elementsPerByte)
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
            return ReadBits(index, ElementsPerByte, BitMask);
        }

        protected sealed override void WriteElement(int value, int index)
        {
            if (value < 0 || value >= 4)
                throw new ArgumentOutOfRangeException(nameof(value), "Quadruple index must be in the range from 0 to 3.");

            WriteBits((byte)value, index, ElementsPerByte, BitMask);
        }

        public QuadrupleIndexArray Clone()
        {
            return new QuadrupleIndexArray(Buffer, Length, ElementsPerByte);
        }
    }
}
