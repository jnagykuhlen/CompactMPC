using System;
using System.Collections.Generic;
using CompactMPC.Circuits.Internal;

namespace CompactMPC.Circuits
{
    public class Circuit : IEvaluableCircuit
    {
        private IReadOnlyList<OutputGate> OutputGates { get; }
        
        public CircuitContext Context { get; }
        
        public Circuit(IReadOnlyList<OutputGate> outputGates, CircuitContext context)
        {
            OutputGates = outputGates;
            Context = context;
        }

        public T[] Evaluate<T>(ICircuitEvaluator<T> evaluator, T[] input)
        {
            if (input.Length != Context.NumberOfInputGates)
                throw new ArgumentException("Number of provided inputs does not match the number of input gates in the circuit.", nameof(input));

            EvaluationState<T> evaluationState = new EvaluationState<T>(input, Context);

            foreach (OutputGate outputGate in OutputGates)
                EvaluateSubtree(outputGate, evaluator, evaluationState);

            return evaluationState.Output;
        }
        
        private void EvaluateSubtree<T>(
            Gate gate,
            ICircuitEvaluator<T> evaluator,
            EvaluationState<T> evaluationState)
        {
            foreach (Gate inputGate in gate.InputGates)
                if (!evaluationState.IsGateEvaluated(inputGate))
                    EvaluateSubtree(inputGate, evaluator, evaluationState);

            gate.Evaluate(evaluator, evaluationState);
        }
    }
}
