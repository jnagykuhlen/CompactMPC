using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public interface IBooleanEvaluable
    {
        T[] Evaluate<T>(IBooleanCircuitEvaluator<T> evaluator, T[] inputValues);
        int NumberOfInputs { get; }
        int NumberOfOutputs { get; }
        CircuitContext CircuitContext { get; }
    }
}
