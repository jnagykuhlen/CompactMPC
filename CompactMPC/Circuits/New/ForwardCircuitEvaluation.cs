using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits.Batching;
using CompactMPC.Circuits.Batching.Internal;

namespace CompactMPC.Circuits.New
{
    public static class ForwardCircuitEvaluation
    {
        public static ForwardCircuitEvaluation<T> From<T>(IBatchCircuitEvaluator<T> evaluator)
        {
            return new ForwardCircuitEvaluation<T>(evaluator);
        }

        public static ForwardCircuitEvaluation<T> From<T>(ICircuitEvaluator<T> evaluator)
        {
            return new ForwardCircuitEvaluation<T>(new BatchCircuitEvaluator<T>(evaluator));
        }
    }

    public class ForwardCircuitEvaluation<T>
    {
        private readonly IBatchCircuitEvaluator<T> _evaluator;
        private readonly Dictionary<Wire, T> _wireInputs;
        private readonly Dictionary<ForwardGate, Wire> _outputWiresByGate;

        public ForwardCircuitEvaluation(IBatchCircuitEvaluator<T> evaluator)
        {
            _evaluator = evaluator;
            _wireInputs = new Dictionary<Wire, T>();
            _outputWiresByGate = new Dictionary<ForwardGate, Wire>();
        }

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

        // TODO: Return custom type instead of Dictionary
        public IReadOnlyDictionary<Wire, T> Execute()
        {
            ForwardEvaluationState<T> evaluationState = new ForwardEvaluationState<T>();
            Dictionary<Wire, T> wireOutputs = new Dictionary<Wire, T>(_outputWiresByGate.Count);
            evaluationState.OnOutputEvaluated += (gate, value) =>
            {
                if (_outputWiresByGate.TryGetValue(gate, out Wire? wire))
                    wireOutputs.Add(wire, value);
            };

            foreach ((Wire wire, T value) in _wireInputs)
                wire.Gate.SendOutputValue(value, _evaluator, evaluationState);

            GateEvaluation<T>[] delayedAndGateEvaluations;
            while ((delayedAndGateEvaluations = evaluationState.NextDelayedAndGateEvaluations()).Length > 0)
            {
                GateEvaluationInput<T>[] evaluationInputs = delayedAndGateEvaluations.Select(evaluation => evaluation.Input).ToArray();
                T[] evaluationOutputs = _evaluator.EvaluateAndGateBatch(evaluationInputs);

                if (evaluationOutputs.Length != evaluationInputs.Length)
                    throw new CircuitEvaluationException("Batch circuit evaluator must provide exactly one output value for each gate evaluation.");

                for (int i = 0; i < delayedAndGateEvaluations.Length; ++i)
                    delayedAndGateEvaluations[i].Gate.SendOutputValue(evaluationOutputs[i], _evaluator, evaluationState);
            }

            if (wireOutputs.Count < _outputWiresByGate.Count)
                throw new CircuitEvaluationException($"Could not evaluate {_outputWiresByGate.Count - wireOutputs.Count} output gate values from given inputs.");

            return wireOutputs;
        }
    }
}