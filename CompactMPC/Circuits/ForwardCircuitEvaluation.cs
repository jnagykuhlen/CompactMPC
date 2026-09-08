using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Circuits.Internal;

namespace CompactMPC.Circuits;

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

            if (evaluationOutputs.Count != evaluationInputs.Length)
                throw new CircuitEvaluationException("Batch circuit evaluator must provide exactly one output value for each gate evaluation.");

            for (var i = 0; i < delayedAndGateEvaluations.Length; ++i)
                delayedAndGateEvaluations[i].Gate.SendOutputValue(evaluationOutputs[i], evaluator, evaluationState);
        }

        return new ForwardCircuitEvaluationResult<T>(evaluationState);
    }
}

public class ForwardCircuitEvaluation(IBatchCircuitVisitor visitor)
{
    public void Execute(IEnumerable<Wire> inputWires)
    {
        var circuitEvaluator = new VisitingCircuitEvaluator(visitor);

        new ForwardCircuitEvaluation<Void>(circuitEvaluator).ExecuteAsync(
            inputWires.Select(wire => new WireValue<Void>(wire, Void.Value)),
            []
        ).Wait();
    }

    private class VisitingCircuitEvaluator(IBatchCircuitVisitor visitor) : IAsyncBatchCircuitEvaluator<Void>
    {
        public Task<IReadOnlyList<Void>> EvaluateAndGateBatchAsync(IReadOnlyList<GateEvaluationInput<Void>> evaluationInputs)
        {
            visitor.VisitAndGateBatch(evaluationInputs.Count);
            return Task.FromResult<IReadOnlyList<Void>>(evaluationInputs.Select(_ => Void.Value).ToList());
        }

        public Void EvaluateXorGate(Void leftValue, Void rightValue)
        {
            visitor.VisitXorGate();
            return Void.Value;
        }

        public Void EvaluateNotGate(Void value)
        {
            visitor.VisitNotGate();
            return Void.Value;
        }
    }

    private class Void
    {
        public static readonly Void Value = new();
        private Void() { }
    }
}
