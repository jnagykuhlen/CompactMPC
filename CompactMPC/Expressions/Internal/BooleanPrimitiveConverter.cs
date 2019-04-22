using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;

namespace CompactMPC.Expressions.Internal
{
    public class BooleanPrimitiveConverter : PrimitiveConverter
    {
        public override SecurePrimitive FromWires(CircuitBuilder builder, IEnumerable<Wire> wires)
        {
            return new SecureBoolean(builder, wires.First());
        }

        public override void WriteInput(object input, BitArray buffer, int startIndex)
        {
            if (input is bool)
            {
                buffer[startIndex] = new Bit((bool)input);
            }
            else
            {
                throw new ArgumentException(String.Format("Input must be of type {0}.", typeof(bool).FullName), nameof(input));
            }
        }

        public override object ReadOutput(BitArray buffer, int startIndex)
        {
            return buffer[startIndex];
        }

        public override int NumberOfWires
        {
            get
            {
                return 1;
            }
        }
    }
}
