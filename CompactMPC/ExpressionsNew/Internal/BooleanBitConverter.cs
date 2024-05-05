using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew.Internal
{
    public class BooleanBitConverter : IBitConverter<bool>
    {
        public static readonly BooleanBitConverter Instance = new();

        public IReadOnlyList<Bit> ToBits(bool value, int numberOfBits) => new BitArray([(Bit)value]);
        public bool FromBits(IReadOnlyList<Bit> bits) => (bool)bits[0];
    }
}
