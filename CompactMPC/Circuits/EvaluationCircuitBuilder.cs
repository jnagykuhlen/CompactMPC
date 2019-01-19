using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CompactMPC.Circuits.Internal;

namespace CompactMPC.Circuits
{
    /// <summary>
    /// Represents a circuit builder that provides means to evaluate the internal circuit after construction.
    /// </summary>
    public class EvaluationCircuitBuilder : CircuitBuilder, IBooleanEvaluable
    {
        private List<int> _outputWireIds;
        private List<int> _inputWireIds;
        private Dictionary<int, Gate> _wireToGateMapping;

        /// <summary>
        /// Creates a new empty circuit builder.
        /// </summary>
        public EvaluationCircuitBuilder()
        {
            _outputWireIds = new List<int>();
            _inputWireIds = new List<int>();
            _wireToGateMapping = new Dictionary<int, Gate>();
        }

        /// <summary>
        /// Records an algorithm given in boolean logic into a new circuit builder and
        /// returns the result for subsequent evaluation.
        /// </summary>
        /// <param name="recorder">Circuit recorder representing an algorithm in boolean logic.</param>
        /// <returns>Circuit builder for evaluation.</returns>
        public static EvaluationCircuitBuilder FromRecorder(ICircuitRecorder recorder)
        {
            EvaluationCircuitBuilder builder = new EvaluationCircuitBuilder();
            recorder.Record(builder);
            return builder;
        }

        protected override void AddAndGate(Wire leftInput, Wire rightInput, Wire output, GateContext context)
        {
            _wireToGateMapping.Add(output.Id, new AndGate(context, leftInput.Id, rightInput.Id));
        }

        protected override void AddXorGate(Wire leftInput, Wire rightInput, Wire output, GateContext context)
        {
            _wireToGateMapping.Add(output.Id, new XorGate(context, leftInput.Id, rightInput.Id));
        }

        protected override void AddNotGate(Wire input, Wire output, GateContext context)
        {
            _wireToGateMapping.Add(output.Id, new NotGate(context, input.Id));
        }
        
        protected override void MakeInputWire(Wire bit)
        {
            _inputWireIds.Add(bit.Id);
        }

        protected override void MakeOutputWire(Wire bit)
        {
            _outputWireIds.Add(bit.Id);
        }

        /// <summary>
        /// Evaluates the internal circuit of this builder locally with specified input bits.
        /// </summary>
        /// <param name="inputValues">Input bits corresponding to each declared input wire.</param>
        /// <returns>Output bits corresponding to each declared output wire.</returns>
        public BitArray Evaluate(BitArray inputValues)
        {
            return new BitArray(Evaluate(new SimpleBooleanCircuitEvaluator(), inputValues.Cast<bool>().ToArray()));
        }

        /// <summary>
        /// Evaluates the internal circuit of this builder using a specified evaluator, where each gate is handled asynchronously.
        /// </summary>
        /// <typeparam name="T">The data type that represents actual wire values.</typeparam>
        /// <param name="evaluator">An abstract evaluator that is called for each gate in the circuit.</param>
        /// <param name="inputValues">Input values corresponding to each declared input wire.</param>
        /// <returns>Output bits corresponding to each declared output wire.</returns>
        public T[] EvaluateParallel<T>(IBooleanCircuitEvaluator<Task<T>> evaluator, T[] inputValues)
        {
            Task<T>[] inputTasks = inputValues.Select(inputValue => Task.FromResult(inputValue)).ToArray();
            return Task.WhenAll(Evaluate(evaluator, inputTasks)).Result;
        }

        /// <summary>
        /// Evaluates the internal circuit of this builder using a specified evaluator, where the gates are processed sequentially.
        /// </summary>
        /// <typeparam name="T">The data type that represents actual wire values.</typeparam>
        /// <param name="evaluator">An abstract evaluator that is called for each gate in the circuit.</param>
        /// <param name="inputValues">Input values corresponding to each declared input wire.</param>
        /// <returns>Output bits corresponding to each declared output wire.</returns>
        public T[] Evaluate<T>(IBooleanCircuitEvaluator<T> evaluator, T[] inputValues)
        {
            if (inputValues.Length != _inputWireIds.Count)
                throw new ArgumentException("Number of input values must match the number of input wires.", nameof(inputValues));

            IdMapping<WireState<T>> wireStates = new IdMapping<WireState<T>>(WireState<T>.Empty, _outputWireIds.Max() + 1);
            for (int i = 0; i < _inputWireIds.Count; ++i)
                wireStates[i] = new WireState<T>(inputValues[i]);

            T[] outputValues = new T[_outputWireIds.Count];
            for (int i = 0; i < _outputWireIds.Count; ++i)
                outputValues[i] = Evaluate(_outputWireIds[i], evaluator, wireStates);

            return outputValues;
        }

        private T Evaluate<T>(int wireId, IBooleanCircuitEvaluator<T> evaluator, IdMapping<WireState<T>> wireStates)
        {
            if (!wireStates[wireId].IsEvaluated)
            {
                wireStates[wireId] = new WireState<T>(default(T));

                Gate gate;
                if (!_wireToGateMapping.TryGetValue(wireId, out gate))
                    throw new ArgumentException("There exists no gate to evaluate for this wire.", nameof(wireId));

                foreach (int inputWireId in gate.InputWireIds)
                    Evaluate(inputWireId, evaluator, wireStates);

                wireStates[wireId] = new WireState<T>(gate.Evaluate(evaluator, wireStates, CircuitContext));
            }

            return wireStates[wireId].Value;
        }

        /// <summary>
        /// Gets the number of input wires in the circuit.
        /// </summary>
        public int NumberOfInputs
        {
            get
            {
                return _inputWireIds.Count;
            }
        }

        /// <summary>
        /// Gets the number of output wires in the circuit.
        /// </summary>
        public int NumberOfOutputs
        {
            get
            {
                return _outputWireIds.Count;
            }
        }
    }
}
