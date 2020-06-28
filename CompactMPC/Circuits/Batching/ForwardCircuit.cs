using System;
using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits.Batching.Internal;

namespace CompactMPC.Circuits.Batching
{
    public class ForwardCircuit : IEvaluableCircuit, IBatchEvaluableCircuit
    {
        private readonly IReadOnlyList<ForwardGate> _inputGates;
        private readonly IReadOnlyList<ForwardGate> _outputGates;
        private CircuitContext? _context;

        public ForwardCircuit(IReadOnlyList<ForwardGate> inputGates, IReadOnlyList<ForwardGate> outputGates)
        {
            if (inputGates.Any(gate => !gate.IsAssignable))
                throw new ArgumentException("Cannot assign input to unassignable gate.", nameof(inputGates));
            
            if (inputGates.Distinct().Count() < inputGates.Count)
                throw new ArgumentException("Cannot assign input gate more than once.", nameof(inputGates));
            
            _inputGates = inputGates;
            _outputGates = outputGates;
            _context = null;
        }

        // TODO: For compatibility only
        public static ForwardCircuit FromCircuit(IEvaluableCircuit circuit)
        {
            return ForwardCircuitBuilder.Build(circuit);
        }

        private static CircuitContext CreateContext(IReadOnlyList<ForwardGate> inputGates, IReadOnlyList<ForwardGate> outputGates)
        {
            CountingCircuitVisitor visitor = new CountingCircuitVisitor();
            ForwardVisitingState visitingState = new ForwardVisitingState();
            
            foreach (ForwardGate inputGate in inputGates)
                inputGate.SendVisitingRequest(visitor, visitingState);
            
            return new CircuitContext(
                visitor.NumberOfAndGates,
                visitor.NumberOfXorGates,
                visitor.NumberOfNotGates,
                inputGates.Count,
                outputGates.Count
            );
        }

        public IReadOnlyList<T> Evaluate<T>(ICircuitEvaluator<T> evaluator, IReadOnlyList<T> input)
        {
            return Evaluate(new BatchCircuitEvaluator<T>(evaluator), input);
        }

        public IReadOnlyList<T> Evaluate<T>(IBatchCircuitEvaluator<T> evaluator, IReadOnlyList<T> input)
        {
            if (input.Count != _inputGates.Count)
                throw new ArgumentException("Number of provided inputs does not match the number of input wires in the circuit.", nameof(input));

            ForwardEvaluationState<T> evaluationState = new ForwardEvaluationState<T>(_outputGates);

            for (int i = 0; i < _inputGates.Count; ++i)
                _inputGates[i].SendOutputValue(input[i], evaluator, evaluationState);

            GateEvaluation<T>[] delayedAndGateEvaluations;
            while ((delayedAndGateEvaluations = evaluationState.NextDelayedAndGateEvaluations()).Length > 0)
            {
                GateEvaluationInput<T>[] evaluationInputs = delayedAndGateEvaluations.Select(evaluation => evaluation.Input).ToArray();
                T[] evaluationOutputs = evaluator.EvaluateAndGateBatch(evaluationInputs);

                if (evaluationOutputs.Length != evaluationInputs.Length)
                    throw new CircuitEvaluationException("Batch circuit evaluator must provide exactly one output value for each gate evaluation.");

                for (int i = 0; i < delayedAndGateEvaluations.Length; ++i)
                    delayedAndGateEvaluations[i].Gate.SendOutputValue(evaluationOutputs[i], evaluator, evaluationState);
            }

            T[] output = new T[_outputGates.Count];
            for (int i = 0; i < output.Length; ++i)
            {
                if (!evaluationState.GetOutputValue(_outputGates[i], out output[i]))
                    throw new CircuitEvaluationException($"Output at index {i + 1} could not be evaluated from the given input.");
            }

            return output;
        }

        public CircuitContext Context {
            get
            {
                if (_context == null)
                    _context = CreateContext(_inputGates, _outputGates);

                return _context;
            }
        }
    }
}
