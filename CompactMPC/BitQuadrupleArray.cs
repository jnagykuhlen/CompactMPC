using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public class BitQuadrupleArray : PackedArray<BitQuadruple>
    {
        private const int ElementsPerByte = 2;
        private const int BitMask = 0xf;

        public BitQuadrupleArray(int numberOfElements)
            : base(numberOfElements, ElementsPerByte) { }

        public BitQuadrupleArray(BitQuadruple[] elements)
            : base(elements, ElementsPerByte) { }

        protected BitQuadrupleArray(byte[] bytes, int numberOfElements)
            : base(bytes, numberOfElements, ElementsPerByte) { }
        
        public static BitQuadrupleArray FromBytes(byte[] bytes, int numberOfElements)
        {
            return new BitQuadrupleArray(bytes, numberOfElements);
        }

        public static int RequiredBytes(int numberOfElements)
        {
            return RequiredBytes(numberOfElements, ElementsPerByte);
        }

        protected override BitQuadruple ReadElement(int index)
        {
            return BitQuadruple.FromPackedValue(ReadBits(index, ElementsPerByte, BitMask));
        }

        protected override void WriteElement(BitQuadruple value, int index)
        {
            WriteBits(value.PackedValue, index, ElementsPerByte, BitMask);
        }
    }
}
