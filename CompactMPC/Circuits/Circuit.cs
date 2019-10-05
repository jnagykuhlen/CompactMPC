using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class Circuit : IEvaluableCircuit
    {
        private Wire[] _inputWires;
        private Wire[] _outputWires;

        public Circuit(Wire[] inputWires, Wire[] outputWires)
        {
            if (inputWires == null)
                throw new ArgumentNullException(nameof(inputWires));

            if (outputWires == null)
                throw new ArgumentNullException(nameof(outputWires));

            _inputWires = inputWires;
            _outputWires = outputWires;
        }

        public T[] Evaluate<T>(ICircuitEvaluator<T> evaluator, T[] input)
        {
            if (input.Length != _inputWires.Length)
                throw new ArgumentException("Number of provided inputs does not match the number of input gates in the circuit.", nameof(input));

            CircuitEvaluation<T> evaluation = new CircuitEvaluation<T>(evaluator);
            for (int i = 0; i < input.Length; ++i)
                evaluation.AssignInputValue(_inputWires[i], input[i]);

            T[] output = new T[_outputWires.Length];
            for (int i = 0; i < output.Length; ++i)
                output[i] = evaluation.GetWireValue(_outputWires[i]);

            return output;
        }

        public IReadOnlyList<Wire> InputWires
        {
            get
            {
                return _inputWires;
            }
        }

        public IReadOnlyList<Wire> OutputWires
        {
            get
            {
                return _outputWires;
            }
        }
    }
}
