using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompactMPC.Circuits;

public interface IAsyncBatchCircuitEvaluator<T>
{
    Task<IReadOnlyList<T>> EvaluateAndGateBatchAsync(IReadOnlyList<GateEvaluationInput<T>> evaluationInputs);
    T EvaluateXorGate(T leftValue, T rightValue);
    T EvaluateNotGate(T value);
}
