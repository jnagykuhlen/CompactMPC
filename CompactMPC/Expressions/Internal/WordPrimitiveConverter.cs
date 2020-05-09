using System;
using System.Collections.Generic;
using CompactMPC.Circuits;

namespace CompactMPC.Expressions.Internal
{
    public class WordPrimitiveConverter : PrimitiveConverter
    {
        public override int NumberOfWires { get; }
        
        public WordPrimitiveConverter(int numberOfWires)
        {
            NumberOfWires = numberOfWires;
        }

        public override SecurePrimitive FromWires(CircuitBuilder builder, IEnumerable<Wire> wires)
        {
            return new SecureWord(builder, wires);
        }

        public override void WriteInput(object input, BitArray buffer, int startIndex)
        {
            if (input is BitArray inputBitArray)
                WriteBitArray(inputBitArray, buffer, startIndex, NumberOfWires);
            else
                throw new ArgumentException($"Input must be of type {typeof(BitArray).FullName}.", nameof(input));
        }
        
        public override object ReadOutput(BitArray buffer, int startIndex)
        {
            return ReadBitArray(buffer, startIndex, NumberOfWires);
        }

        private static void WriteBitArray(BitArray value, BitArray buffer, int startIndex, int length)
        {
            if (value.Length != length)
                throw new ArgumentException("Length of input array does not match with number of wires.", nameof(value));

            for (int i = 0; i < length; ++i)
                buffer[startIndex + i] = value[i];
        }

        private static BitArray ReadBitArray(BitArray buffer, int startIndex, int length)
        {
            BitArray value = new BitArray(length);
            for (int i = 0; i < length; ++i)
                value[i] = buffer[startIndex + i];

            return value;
        }
    }
}
