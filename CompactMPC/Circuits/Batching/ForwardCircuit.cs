using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits.Batching.Internal;

namespace CompactMPC.Circuits.Batching
{
    public class ForwardCircuit : IEvaluableCircuit
    {
        private IReadOnlyList<ForwardGate> _activeInputGates;
        private CircuitContext _context;

        public ForwardCircuit(IEvaluableCircuit circuit)
        {
            _activeInputGates = ForwardCircuitBuilder.Build(circuit);
            _context = circuit.Context;
        }

        /*
        public T[] Evaluate<T>(ICircuitEvaluator<T> evaluator, T[] input)
        {
            CircuitEvaluationState<T> evaluationState = new CircuitEvaluationState<T>(input, _context);

            Queue<IForwardGate> currentGates = new Queue<IForwardGate>(ActiveInputGates);
            while (currentGates.Count > 0)
            {
                IForwardGate gate = currentGates.Dequeue();
                if (!evaluationState.IsGateEvaluated(gate.InnerGate) && gate.InnerGate.InputGates.All(inputGate => evaluationState.IsGateEvaluated(inputGate)))
                {
                    gate.InnerGate.Evaluate(evaluator, evaluationState, _context);
                    foreach (IForwardGate successor in gate.Successors)
                        currentGates.Enqueue(successor);
                }
            }

            return evaluationState.Output;
        }
        */

        public T[] Evaluate<T>(ICircuitEvaluator<T> evaluator, T[] input)
        {
            return Evaluate(new BatchedCircuitEvaluator<T>(evaluator), input);
        }

        public T[] Evaluate<T>(IBatchedCircuitEvaluator<T> evaluator, T[] input)
        {
            ForwardEvaluationState<T> evaluationState = new ForwardEvaluationState<T>(input, _context.NumberOfOutputGates);

            foreach (ForwardGate gate in _activeInputGates)
                gate.Evaluate(evaluator, evaluationState, _context);

            BatchElement<T>[] currentAndGateBatch;
            while ((currentAndGateBatch = evaluationState.GetNextAndGateBatch()).Length > 0)
            {
                GateEvaluationInput<T>[] evaluationInputs = currentAndGateBatch.Select(batchElement => batchElement.EvaluationInput).ToArray();
                T[] batchOutputValues = evaluator.EvaluateAndGateBatch(evaluationInputs, _context);

                if (batchOutputValues.Length != currentAndGateBatch.Length)
                    throw new ArgumentException("Batched circuit evaluator must provide exactly one output value for each gate evaluation.", nameof(evaluator));

                for (int i = 0; i < currentAndGateBatch.Length; ++i)
                {
                    foreach (ForwardGate successor in currentAndGateBatch[i].Successors)
                    {
                        evaluationState.PushInputValue(successor, batchOutputValues[i]);
                        if (evaluationState.GetNumberOfInputValues(successor) >= successor.NumberOfInputs)
                            successor.Evaluate(evaluator, evaluationState, _context);
                    }
                }
            }

            return evaluationState.Output;
        }

        public CircuitContext Context
        {
            get
            {
                return _context;
            }
        }
    }
}
