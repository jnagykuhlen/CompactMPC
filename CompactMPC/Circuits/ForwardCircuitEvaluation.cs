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

public static class ForwardCircuitEvaluation
{
    public static CircuitStatistics CreateStatistics(IEnumerable<Wire> inputWires, IEnumerable<Wire> outputWires)
    {
        var circuitEvaluator = new StatisticsCircuitEvaluator();

        new ForwardCircuitEvaluation<Void>(circuitEvaluator).ExecuteAsync(
            inputWires.Select(wire => new WireValue<Void>(wire, Void.Value)),
            outputWires
        ).Wait();

        return circuitEvaluator.GetStatistics();
    }

    private class StatisticsCircuitEvaluator : IAsyncBatchCircuitEvaluator<Void>
    {
        private int _numberOfAndGates;
        private int _numberOfXorGates;
        private int _numberOfNotGates;
        private int _multiplicativeDepth;

        public Task<IReadOnlyList<Void>> EvaluateAndGateBatchAsync(IReadOnlyList<GateEvaluationInput<Void>> evaluationInputs)
        {
            _numberOfAndGates += evaluationInputs.Count;
            _multiplicativeDepth++;
            return Task.FromResult<IReadOnlyList<Void>>(evaluationInputs.Select(_ => Void.Value).ToList());
        }

        public Void EvaluateXorGate(Void leftValue, Void rightValue)
        {
            _numberOfXorGates++;
            return Void.Value;
        }

        public Void EvaluateNotGate(Void value)
        {
            _numberOfNotGates++;
            return Void.Value;
        }

        public CircuitStatistics GetStatistics() =>
            new(_numberOfAndGates, _numberOfXorGates, _numberOfNotGates, _multiplicativeDepth);
    }

    private class Void
    {
        public static readonly Void Value = new();
        private Void() { }
    }
}
