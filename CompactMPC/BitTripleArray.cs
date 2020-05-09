namespace CompactMPC
{
    public class BitTripleArray : PackedArray<BitTriple>
    {
        public BitTripleArray(int numberOfElements)
            : base(RequiredBytes(numberOfElements), numberOfElements) { }

        public BitTripleArray(BitTriple[] elements)
            : base(RequiredBytes(elements.Length), elements) { }

        protected BitTripleArray(byte[] bytes, int numberOfElements)
            : base(bytes, RequiredBytes(numberOfElements), numberOfElements) { }

        public static BitTripleArray FromBytes(byte[] bytes, int numberOfElements)
        {
            return new BitTripleArray(bytes, numberOfElements);
        }

        public static int RequiredBytes(int numberOfElements)
        {
            if (numberOfElements > 0)
                return (3 * numberOfElements - 1) / 8 + 1;

            return 0;
        }

        protected override BitTriple ReadElement(int index)
        {
            int startBitIndex = 3 * index;
            return new BitTriple(
                (Bit)ReadBit(startBitIndex + 0),
                (Bit)ReadBit(startBitIndex + 1),
                (Bit)ReadBit(startBitIndex + 2)
            );
        }

        protected override void WriteElement(BitTriple value, int index)
        {
            int startBitIndex = 3 * index;
            WriteBit((byte)value.X, startBitIndex + 0);
            WriteBit((byte)value.Y, startBitIndex + 1);
            WriteBit((byte)value.Z, startBitIndex + 2);
        }

        public BitTripleArray Clone()
        {
            return new BitTripleArray(Buffer, Length);
        }
    }
}
