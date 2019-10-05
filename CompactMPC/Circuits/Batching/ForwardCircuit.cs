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

        public ForwardCircuit(Wire[] inputWires, Wire[] outputWires)
        {
            ForwardCircuitBuildingEvaluator evaluator = new ForwardCircuitBuildingEvaluator();
            CircuitEvaluation<ForwardGate> evaluation = new CircuitEvaluation<ForwardGate>(evaluator);

            _inputGates = new ForwardGate[inputWires.Length];
            for (int i = 0; i < inputWires.Length; ++i)
            {
                _inputGates[i] = new ForwardInputGate();
                evaluation.AssignInputValue(inputWires[i], _inputGates[i]);
            }

            for (int i = 0; i < outputWires.Length; ++i)
            {
                ForwardGate outputGate = evaluation.GetWireValue(outputWires[i]);
                outputGate.AddSuccessor(new ForwardOutputGate(i));
            }

            _context = new CircuitContext(
                evaluator.NumberOfAndGates,
                evaluator.NumberOfXorGates,
                evaluator.NumberOfNotGates,
                inputWires.Length,
                outputWires.Length
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

            ForwardEvaluationState<T> evaluationState = new ForwardEvaluationState<T>(_context.NumberOfOutputs);

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

        public CircuitContext Context
        {
            get
            {
                return _context;
            }
        }
    }
}
