using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching
{
    public interface IBatchEvaluableCircuit
    {
        T[] Evaluate<T>(IBatchCircuitEvaluator<T> evaluator, T[] inputValues);
        CircuitContext Context { get; }
    }
}
