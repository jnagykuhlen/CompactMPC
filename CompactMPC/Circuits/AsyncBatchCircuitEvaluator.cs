using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompactMPC.Circuits;

public class AsyncBatchCircuitEvaluator<T>(ICircuitEvaluator<T> innerEvaluator) : IAsyncBatchCircuitEvaluator<T>
{
    public Task<IReadOnlyList<T>> EvaluateAndGateBatchAsync(IReadOnlyList<GateEvaluationInput<T>> evaluationInputs)
    {
        var outputValues = new T[evaluationInputs.Count];
        for (var i = 0; i < evaluationInputs.Count; ++i)
        {
            var evaluationInput = evaluationInputs[i];
            outputValues[i] = innerEvaluator.EvaluateAndGate(
                evaluationInput.LeftValue,
                evaluationInput.RightValue
            );
        }

        return Task.FromResult<IReadOnlyList<T>>(outputValues);
    }

    public T EvaluateXorGate(T leftValue, T rightValue) => innerEvaluator.EvaluateXorGate(leftValue, rightValue);
    public T EvaluateNotGate(T value) => innerEvaluator.EvaluateNotGate(value);
}
