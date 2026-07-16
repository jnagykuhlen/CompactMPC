using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits.Batching;
using CompactMPC.Circuits.Batching.Internal;

namespace CompactMPC.Circuits.New
{
    public static class ForwardCircuitEvaluation
    {
        public static ForwardCircuitEvaluation<T> From<T>(IBatchCircuitEvaluator<T> evaluator) => new(evaluator);

        public static ForwardCircuitEvaluation<T> From<T>(ICircuitEvaluator<T> evaluator) =>
            new(new BatchCircuitEvaluator<T>(evaluator));
    }

    public class ForwardCircuitEvaluation<T>(IBatchCircuitEvaluator<T> evaluator)
    {
        private readonly Dictionary<Wire, T> _wireInputs = new();
        private readonly Dictionary<ForwardGate, Wire> _outputWiresByGate = new();

        public ForwardCircuitEvaluation<T> Input(WireValue<T> wireValue)
        {
            if (!_wireInputs.TryAdd(wireValue.Wire, wireValue.Value))
                throw new CircuitEvaluationException("Wire value has already been assigned.");

            return this;
        }

        public ForwardCircuitEvaluation<T> Input(IEnumerable<WireValue<T>> wireValues)
        {
            foreach (WireValue<T> wireValue in wireValues)
                Input(wireValue);

            return this;
        }

        public ForwardCircuitEvaluation<T> Output(Wire wire)
        {
            _outputWiresByGate.Add(wire.Gate, wire);
            return this;
        }

        public ForwardCircuitEvaluation<T> Output(IEnumerable<Wire> wires)
        {
            foreach (Wire wire in wires)
                Output(wire);

            return this;
        }

        public ForwardCircuitEvaluationResult<T> Execute()
        {
            var evaluationState = new ForwardEvaluationState<T>();
            var wireOutputs = new Dictionary<Wire, T>(_outputWiresByGate.Count);
            evaluationState.OnOutputEvaluated += (gate, value) =>
            {
                if (_outputWiresByGate.TryGetValue(gate, out var wire))
                    wireOutputs.Add(wire, value);
            };

            foreach (var (wire, value) in _wireInputs.AsEnumerable().OrderBy(wire => wire.Key.Id))
                wire.Gate.SendOutputValue(value, evaluator, evaluationState);

            GateEvaluation<T>[] delayedAndGateEvaluations;
            while ((delayedAndGateEvaluations = evaluationState.NextDelayedAndGateEvaluations()).Length > 0)
            {
                var evaluationInputs = delayedAndGateEvaluations
                    .Select(evaluation => evaluation.Input)
                    .ToArray();
                
                var evaluationOutputs = evaluator.EvaluateAndGateBatch(evaluationInputs);

                if (evaluationOutputs.Length != evaluationInputs.Length)
                    throw new CircuitEvaluationException("Batch circuit evaluator must provide exactly one output value for each gate evaluation.");

                for (var i = 0; i < delayedAndGateEvaluations.Length; ++i)
                    delayedAndGateEvaluations[i].Gate.SendOutputValue(evaluationOutputs[i], evaluator, evaluationState);
            }

            if (wireOutputs.Count < _outputWiresByGate.Count)
                throw new CircuitEvaluationException($"Could not evaluate {_outputWiresByGate.Count - wireOutputs.Count} output gate values from given inputs.");

            return new ForwardCircuitEvaluationResult<T>(wireOutputs);
        }
    }
}