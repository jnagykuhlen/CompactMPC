using System;
using System.Linq;
using CompactMPC.Circuits.Batching.Internal;

namespace CompactMPC.Circuits.Batching
{
    public class ForwardCircuit : IEvaluableCircuit, IBatchEvaluableCircuit
    {
        private ForwardGate[] InputGates { get; }

        public CircuitContext Context { get; }

        public ForwardCircuit(IEvaluableCircuit circuit)
        {
            InputGates = ForwardCircuitBuilder.Build(circuit);
            Context = circuit.Context;
        }
        
        public T[] Evaluate<T>(ICircuitEvaluator<T> evaluator, T[] input)
        {
            return Evaluate(new BatchCircuitEvaluator<T>(evaluator), input);
        }

        public T[] Evaluate<T>(IBatchCircuitEvaluator<T> evaluator, T[] input)
        {
            if (input.Length != InputGates.Length)
                throw new ArgumentException("Number of provided inputs does not match the number of input gates in the circuit.", nameof(input));

            ForwardEvaluationState<T> evaluationState = new ForwardEvaluationState<T>(Context.NumberOfOutputGates);

            for (int i = 0; i < InputGates.Length; ++i)
                InputGates[i].ReceiveInputValue(input[i], evaluator, evaluationState);

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
