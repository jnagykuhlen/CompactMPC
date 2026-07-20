using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Circuits.Batching;
using CompactMPC.Circuits.Batching.Internal;

namespace CompactMPC.Circuits.New;

public class ForwardCircuitEvaluation<T>(IAsyncBatchCircuitEvaluator<T> evaluator)
{
    public async Task<ForwardCircuitEvaluationResult<T>> ExecuteAsync(IEnumerable<WireValue<T>> inputWireValues, IEnumerable<Wire> outputWires)
    {
        var evaluationState = new ForwardEvaluationState<T>(outputWires.Select(wire => wire.Gate));
        
        foreach (var input in inputWireValues)
            input.Wire.Gate.SendOutputValue(input.Value, evaluator, evaluationState);

        GateEvaluation<T>[] delayedAndGateEvaluations;
        while ((delayedAndGateEvaluations = evaluationState.NextDelayedAndGateEvaluations()).Length > 0)
        {
            var evaluationInputs = delayedAndGateEvaluations
                .Select(evaluation => evaluation.Input)
                .ToArray();
                
            var evaluationOutputs = await evaluator.EvaluateAndGateBatchAsync(evaluationInputs);

            if (evaluationOutputs.Length != evaluationInputs.Length)
                throw new CircuitEvaluationException("Batch circuit evaluator must provide exactly one output value for each gate evaluation.");

            for (var i = 0; i < delayedAndGateEvaluations.Length; ++i)
                delayedAndGateEvaluations[i].Gate.SendOutputValue(evaluationOutputs[i], evaluator, evaluationState);
        }

        return new ForwardCircuitEvaluationResult<T>(evaluationState);
    }
}
