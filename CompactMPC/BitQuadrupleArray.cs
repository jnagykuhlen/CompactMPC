namespace CompactMPC
{
    public class BitQuadrupleArray : PackedArray<BitQuadruple>
    {
        private const int ElementsPerByte = 2;
        private const int BitMask = 0xf;

        public BitQuadrupleArray(int numberOfElements)
            : base(RequiredBytes(numberOfElements), numberOfElements) { }

        public BitQuadrupleArray(BitQuadruple[] elements)
            : base(RequiredBytes(elements.Length), elements) { }

        protected BitQuadrupleArray(byte[] bytes, int numberOfElements)
            : base(bytes, RequiredBytes(numberOfElements), numberOfElements) { }

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

        public BitQuadrupleArray Clone()
        {
            return new BitQuadrupleArray(Buffer, Length);
        }
    }
}
