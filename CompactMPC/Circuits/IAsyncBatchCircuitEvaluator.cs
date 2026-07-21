using System.Threading.Tasks;

namespace CompactMPC.Circuits;

public interface IAsyncBatchCircuitEvaluator<T>
{
    Task<T[]> EvaluateAndGateBatchAsync(GateEvaluationInput<T>[] evaluationInputs);
    T EvaluateXorGate(T leftValue, T rightValue);
    T EvaluateNotGate(T value);
}
