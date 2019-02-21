using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits.Gates;

namespace CompactMPC.Circuits
{
    public class Circuit
    {
        private IReadOnlyList<OutputGate> _outputGates;
        private CircuitContext _context;

        public Circuit(IReadOnlyList<OutputGate> outputGates, CircuitContext context)
        {
            _outputGates = outputGates;
            _context = context;
        }

        public TOut[] Evaluate<TIn, TProcess, TOut>(ICircuitEvaluator<TIn, TProcess, TOut> evaluator, TIn[] input)
        {
            CircuitEvaluationState<TIn, TProcess, TOut> evaluationState = new CircuitEvaluationState<TIn, TProcess, TOut>(input, _context);

            foreach (OutputGate outputGate in _outputGates)
                EvaluateSubtree(outputGate, evaluator, evaluationState);

            return evaluationState.Output;
        }
        
        private void EvaluateSubtree<TIn, TProcess, TOut>(
            Gate gate,
            ICircuitEvaluator<TIn, TProcess, TOut> evaluator,
            CircuitEvaluationState<TIn, TProcess, TOut> evaluationState)
        {
            foreach (Gate inputGate in gate.InputGates)
            {
                if (!evaluationState.IsGateEvaluated(inputGate))
                    EvaluateSubtree(inputGate, evaluator, evaluationState);
            }

            gate.Evaluate(evaluator, evaluationState, _context);
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
