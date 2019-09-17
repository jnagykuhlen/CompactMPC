using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits.Internal;

namespace CompactMPC.Circuits
{
    public class Circuit : IEvaluableCircuit
    {
        private IReadOnlyList<OutputGate> _outputGates;
        private CircuitContext _context;

        public Circuit(IReadOnlyList<OutputGate> outputGates, CircuitContext context)
        {
            _outputGates = outputGates;
            _context = context;
        }

        public T[] Evaluate<T>(ICircuitEvaluator<T> evaluator, T[] input)
        {
            if (input.Length != _context.NumberOfInputGates)
                throw new ArgumentException("Number of provided inputs does not match the number of input gates in the circuit.", nameof(input));

            EvaluationState<T> evaluationState = new EvaluationState<T>(input, _context);

            foreach (OutputGate outputGate in _outputGates)
                EvaluateSubtree(outputGate, evaluator, evaluationState);

            return evaluationState.Output;
        }
        
        private void EvaluateSubtree<T>(
            Gate gate,
            ICircuitEvaluator<T> evaluator,
            EvaluationState<T> evaluationState)
        {
            foreach (Gate inputGate in gate.InputGates)
            {
                if (!evaluationState.IsGateEvaluated(inputGate))
                    EvaluateSubtree(inputGate, evaluator, evaluationState);
            }

            gate.Evaluate(evaluator, evaluationState);
        }

        public IReadOnlyList<OutputGate> OutputGates
        {
            get
            {
                return _outputGates;
            }
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
