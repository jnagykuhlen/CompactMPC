using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching;

public interface IAsyncBatchEvaluableCircuit
{
    Task<IReadOnlyList<T>> EvaluateAsync<T>(IAsyncBatchCircuitEvaluator<T> evaluator, IReadOnlyList<T> inputValues);
    CircuitContext Context { get; }
}
