using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits.Batching;
using CompactMPC.Circuits.Batching.Internal;

namespace CompactMPC.Circuits.New
{
    public class ForwardCircuitEvaluation<T>
    {
        private readonly IBatchCircuitEvaluator<T> _evaluator;
        private readonly Dictionary<ForwardGate, T> _gateInputs;
        private readonly HashSet<ForwardGate> _outputGates;

        private ForwardCircuitEvaluation(IBatchCircuitEvaluator<T> evaluator)
        {
            _evaluator = evaluator;
            _gateInputs = new Dictionary<ForwardGate, T>();
            _outputGates = new HashSet<ForwardGate>();
        }

        public static ForwardCircuitEvaluation<T> From(IBatchCircuitEvaluator<T> evaluator)
        {
            return new ForwardCircuitEvaluation<T>(evaluator);
        }

        public static ForwardCircuitEvaluation<T> From(ICircuitEvaluator<T> evaluator)
        {
            return new ForwardCircuitEvaluation<T>(new BatchCircuitEvaluator<T>(evaluator));
        }

        public ForwardCircuitEvaluation<T> Input(ForwardGate gate, T value)
        {
            if (!gate.IsAssignable)
                throw new CircuitEvaluationException("Cannot assign input to unassignable gate.");

            if (!_gateInputs.TryAdd(gate, value))
                throw new CircuitEvaluationException("Gate value has already been assigned.");

            return this;
        }

        public ForwardCircuitEvaluation<T> Output(ForwardGate gate)
        {
            _outputGates.Add(gate);
            return this;
        }

        public ForwardCircuitEvaluation<T> Output(IEnumerable<ForwardGate> gates)
        {
            foreach (ForwardGate gate in gates)
                _outputGates.Add(gate);
            return this;
        }

        public IReadOnlyDictionary<ForwardGate, T> Execute()
        {
            ForwardEvaluationState<T> evaluationState = new ForwardEvaluationState<T>();
            Dictionary<ForwardGate, T> gateOutputs = new Dictionary<ForwardGate, T>(_outputGates.Count);
            evaluationState.OnOutputEvaluated += (gate, value) =>
            {
                if (_outputGates.Contains(gate))
                    gateOutputs.Add(gate, value);
            };

            foreach ((ForwardGate gate, T value) in _gateInputs)
                gate.SendOutputValue(value, _evaluator, evaluationState);

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

            if (gateOutputs.Count < _outputGates.Count)
                throw new CircuitEvaluationException($"Could not evaluate {_outputGates.Count - gateOutputs.Count} output gate values from given inputs.");

            return gateOutputs;
        }
    }
}