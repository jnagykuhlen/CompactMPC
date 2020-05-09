using System;
using System.Collections.Generic;
using System.Numerics;
using CompactMPC.Circuits;

namespace CompactMPC.Expressions.Internal
{
    public class IntegerPrimitiveConverter : PrimitiveConverter
    {
        public override int NumberOfWires { get; }
        
        public IntegerPrimitiveConverter(int numberOfWires)
        {
            NumberOfWires = numberOfWires;
        }

        public override SecurePrimitive FromWires(CircuitBuilder builder, IEnumerable<Wire> wires)
        {
            return new SecureInteger(builder, wires);
        }

        public override void WriteInput(object input, BitArray buffer, int startIndex)
        {
            if (input is BigInteger inputInteger)
                WriteInteger(inputInteger, buffer, startIndex, NumberOfWires);
            else
                throw new ArgumentException($"Input must be of type {typeof(BigInteger).FullName}.", nameof(input));
        }
        
        public override object ReadOutput(BitArray buffer, int startIndex)
        {
            return ReadInteger(buffer, startIndex, NumberOfWires);
        }

        private static void WriteInteger(BigInteger value, BitArray buffer, int startIndex, int length)
        {
            if (value < 0)
                throw new ArgumentException("Cannot write negative integer.", nameof(value));

            if (value <= ulong.MaxValue)
            {
                ulong fixedValue = (ulong)value;
                for (int i = 0; i < length; ++i)
                    buffer[startIndex + i] = new Bit((fixedValue & (1UL << i)) != 0);
            }
            else
            {
                for (int i = 0; i < length; ++i)
                    buffer[startIndex + i] = new Bit((value & (BigInteger.One << i)) != 0);
            }
        }

        private static BigInteger ReadInteger(BitArray buffer, int startIndex, int length)
        {
            ulong fixedValue = 0;
            for (int i = 0; i < length && i < 64; ++i)
                if (buffer[startIndex + i])
                    fixedValue |= 1UL << i;

            BigInteger value = new BigInteger(fixedValue);

            if (length >= 64)
                for (int i = 64; i < length; ++i)
                    if (buffer[startIndex + i])
                        value |= BigInteger.One << i;

            return value;
        }
    }
}
