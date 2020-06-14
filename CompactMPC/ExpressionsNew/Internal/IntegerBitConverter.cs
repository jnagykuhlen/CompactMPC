using System;
using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew.Internal
{
    public class IntegerBitConverter : IBitConverter<int>
    {
        public static readonly IntegerBitConverter Instance = new IntegerBitConverter();

        public IReadOnlyList<Bit> ToBits(int value, int numberOfBits)
        {
            if (value >= 1 << numberOfBits)
                throw new ArgumentException($"Integer {value} is too large to represent by {numberOfBits} bits.", nameof(value));
            
            BitArray result = new BitArray(numberOfBits);
            for (int i = 0; i < numberOfBits; ++i)
                result[i] = new Bit((value & (1 << i)) != 0);

            return result;
        }

        public int FromBits(IReadOnlyList<Bit> bits)
        {
            int maxNumberOfBits = 8 * sizeof(int);
            if (bits.Count > maxNumberOfBits)
                throw new ArgumentException($"Can not convert more than {maxNumberOfBits} bits to integer.", nameof(bits));
            
            int result = 0;
            for (int i = 0; i < bits.Count; ++i)
            {
                if (bits[i].IsSet)
                    result |= 1 << i;
            }

            return result;
        }
    }
}
