using System;
using System.Linq;
using CompactMPC.Circuits.Batching.Internal;

namespace CompactMPC.Circuits.Batching
{
    public class ForwardCircuit : IEvaluableCircuit, IBatchEvaluableCircuit
    {
        private readonly ForwardGate[] _inputGates;

        public CircuitContext Context { get; }

        public ForwardCircuit(IEvaluableCircuit circuit)
        {
            _inputGates = ForwardCircuitBuilder.Build(circuit);
            Context = circuit.Context;
        }

        public ForwardCircuit(ForwardGate[] inputGates)
        {
            _inputGates = inputGates;
            Context = CreateContext(inputGates);
        }

        private static CircuitContext CreateContext(ForwardGate[] inputGates)
        {
            CountingCircuitVisitor visitor = new CountingCircuitVisitor();
            ForwardVisitingState visitingState = new ForwardVisitingState();
            
            foreach (ForwardGate inputGate in inputGates)
                inputGate.ReceiveVisitingRequest(visitor, visitingState);
            
            return new CircuitContext(
                visitor.NumberOfAndGates,
                visitor.NumberOfXorGates,
                visitor.NumberOfNotGates,
                visitor.NumberOfInputGates,
                visitor.NumberOfOutputGates
            );
        }
        
        public T[] Evaluate<T>(ICircuitEvaluator<T> evaluator, T[] input)
        {
            return Evaluate(new BatchCircuitEvaluator<T>(evaluator), input);
        }

        public T[] Evaluate<T>(IBatchCircuitEvaluator<T> evaluator, T[] input)
        {
            if (input.Length != _inputGates.Length)
                throw new ArgumentException("Number of provided inputs does not match the number of input gates in the circuit.", nameof(input));

            ForwardEvaluationState<T> evaluationState = new ForwardEvaluationState<T>(Context.NumberOfOutputGates);

            for (int i = 0; i < _inputGates.Length; ++i)
                _inputGates[i].ReceiveInputValue(input[i], evaluator, evaluationState);

            GateEvaluation<T>[] delayedAndGateEvaluations;
            while ((delayedAndGateEvaluations = evaluationState.GetDelayedAndGateEvaluations()).Length > 0)
            {
                GateEvaluationInput<T>[] evaluationInputs = delayedAndGateEvaluations.Select(evaluation => evaluation.Input).ToArray();
                T[] evaluationOutputs = evaluator.EvaluateAndGateBatch(evaluationInputs);

                if (evaluationOutputs.Length != evaluationInputs.Length)
                    throw new ArgumentException("Batch circuit evaluator must provide exactly one output value for each gate evaluation.", nameof(evaluator));

                for (int i = 0; i < delayedAndGateEvaluations.Length; ++i)
                    delayedAndGateEvaluations[i].Gate.SendOutputValue(evaluationOutputs[i], evaluator, evaluationState);
            }

            return evaluationState.Output;
        }
    }
}
