using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;
using CompactMPC.Expressions.Internal;

namespace CompactMPC.Expressions
{
    public abstract class PrimitiveConverter
    {
        public static PrimitiveConverter Boolean()
        {
            return new BooleanPrimitiveConverter();
        }

        public static PrimitiveConverter Word(int length)
        {
            return new WordPrimitiveConverter(length);
        }

        public static PrimitiveConverter Integer(int length)
        {
            return new IntegerPrimitiveConverter(length);
        }

        public abstract SecurePrimitive FromWires(Wire[] wires);
        public abstract void WriteInput(object input, BitArray buffer, int startIndex);
        public abstract object ReadOutput(BitArray buffer, int startIndex);
        public abstract int NumberOfWires { get; }
    }
}
