using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class LocalCircuitEvaluator : ICircuitEvaluator<Bit>
    {
        public Bit EvaluateAndGate(Bit leftValue, Bit rightValue)
        {
            return leftValue && rightValue;
        }

        public Bit EvaluateXorGate(Bit leftValue, Bit rightValue)
        {
            return leftValue ^ rightValue;
        }

        public Bit EvaluateNotGate(Bit value)
        {
            return ~value;
        }
    }
}
