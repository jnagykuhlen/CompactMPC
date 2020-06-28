using System.Collections.Generic;

namespace CompactMPC.Circuits.Batching
{
    public interface IBatchEvaluableCircuit
    {
        IReadOnlyList<T> Evaluate<T>(IBatchCircuitEvaluator<T> evaluator, IReadOnlyList<T> inputValues);
        CircuitContext Context { get; }
    }
}
