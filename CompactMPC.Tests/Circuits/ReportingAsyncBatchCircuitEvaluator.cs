using System.Collections.Generic;
using System.Threading.Tasks;
using CompactMPC.Circuits.Batching;

namespace CompactMPC.Circuits;

public class ReportingAsyncBatchCircuitEvaluator<T>(IAsyncBatchCircuitEvaluator<T> innerEvaluator) : IAsyncBatchCircuitEvaluator<T>
{
    private readonly List<int> _batchSizes = new();

    public Task<T[]> EvaluateAndGateBatchAsync(GateEvaluationInput<T>[] evaluationInputs)
    {
        _batchSizes.Add(evaluationInputs.Length);
        return innerEvaluator.EvaluateAndGateBatchAsync(evaluationInputs);
    }

    public T EvaluateXorGate(T leftValue, T rightValue) => innerEvaluator.EvaluateXorGate(leftValue, rightValue);
    public T EvaluateNotGate(T value) => innerEvaluator.EvaluateNotGate(value);

    public int[] BatchSizes => _batchSizes.ToArray();
}
