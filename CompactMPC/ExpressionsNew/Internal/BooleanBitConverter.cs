using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew.Internal
{
    public class BooleanBitConverter : IBitConverter<bool>
    {
        public static readonly BooleanBitConverter Instance = new BooleanBitConverter();

        public IReadOnlyList<Bit> ToBits(bool value, int numberOfBits)
        {
            return new BitArray(1) { [0] = (Bit) value };
        }

        public bool FromBits(IReadOnlyList<Bit> bits)
        {
            return (bool)bits[0];
        }
    }
}
