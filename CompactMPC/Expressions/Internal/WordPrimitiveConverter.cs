using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;

namespace CompactMPC.Expressions.Internal
{
    public class WordPrimitiveConverter : PrimitiveConverter
    {
        private int _numberOfWires;

        public WordPrimitiveConverter(int numberOfWires)
        {
            _numberOfWires = numberOfWires;
        }

        public override SecurePrimitive FromWires(Wire[] wires)
        {
            return new SecureWord(wires);
        }

        public override void WriteInput(object input, BitArray buffer, int startIndex)
        {
            if (input is BitArray)
            {
                WriteBitArray((BitArray)input, buffer, startIndex, NumberOfWires);
            }
            else
            {
                throw new ArgumentException(String.Format("Input must be of type {0}.", typeof(BitArray).FullName), nameof(input));
            }
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

        public override int NumberOfWires
        {
            get
            {
                return _numberOfWires;
            }
        }
    }
}
