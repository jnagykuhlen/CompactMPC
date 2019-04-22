using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits.Batching.Internal;

namespace CompactMPC.Circuits.Batching
{
    public class ForwardCircuit : IEvaluableCircuit, IBatchEvaluableCircuit
    {
        private ForwardGate[] _inputGates;
        private CircuitContext _context;

        public ForwardCircuit(IEvaluableCircuit circuit)
        {
            _inputGates = ForwardCircuitBuilder.Build(circuit);
            _context = circuit.Context;
        }
        
        public T[] Evaluate<T>(ICircuitEvaluator<T> evaluator, T[] input)
        {
            return Evaluate(new BatchCircuitEvaluator<T>(evaluator), input);
        }

        public T[] Evaluate<T>(IBatchCircuitEvaluator<T> evaluator, T[] input)
        {
            if (input.Length != _inputGates.Length)
                throw new ArgumentException("Number of provided inputs does not match the number of input gates in the circuit.", nameof(input));

            ForwardEvaluationState<T> evaluationState = new ForwardEvaluationState<T>(_context.NumberOfOutputGates);

            for (int i = 0; i < _inputGates.Length; ++i)
                _inputGates[i].ReceiveInputValue(input[i], evaluator, evaluationState, _context);

            GateEvaluation<T>[] delayedAndGateEvaluations;
            while ((delayedAndGateEvaluations = evaluationState.GetDelayedAndGateEvaluations()).Length > 0)
            {
                GateEvaluationInput<T>[] evaluationInputs = delayedAndGateEvaluations.Select(evaluation => evaluation.Input).ToArray();
                T[] evaluationOutputs = evaluator.EvaluateAndGateBatch(evaluationInputs, _context);

                if (evaluationOutputs.Length != evaluationInputs.Length)
                    throw new ArgumentException("Batch circuit evaluator must provide exactly one output value for each gate evaluation.", nameof(evaluator));

                for (int i = 0; i < delayedAndGateEvaluations.Length; ++i)
                    delayedAndGateEvaluations[i].Gate.SendOutputValue(evaluationOutputs[i], evaluator, evaluationState, _context);
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
