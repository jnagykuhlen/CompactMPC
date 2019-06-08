using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits.Internal;

namespace CompactMPC.Circuits
{
    /// <summary>
    /// Represents an abstract builder for constructing boolean circuits ad hoc from
    /// AND, XOR and NOT gates. Derived classes need to translate these gates into
    /// an appropriate internal circuit representation.
    /// </summary>
    public class CircuitBuilder
    {
        private int _nextGateId;
        private int _nextAndGateId;
        private int _nextXorGateId;
        private int _nextNotGateId;
        private int _nextInputGateId;
        private int _nextOutputGateId;
        private List<OutputGate> _outputGates;

        /// <summary>
        /// Creates a new empty circuit builder.
        /// </summary>
        public CircuitBuilder()
        {
            _nextGateId = 0;
            _nextAndGateId = 0;
            _nextXorGateId = 0;
            _nextNotGateId = 0;
            _outputGates = new List<OutputGate>();
        }

        /// <summary>
        /// Adds a binary AND gate to the circuit.
        /// </summary>
        /// <param name="leftInput">Left input wire.</param>
        /// <param name="rightInput">Right input wire.</param>
        /// <returns>A wire representing the logical AND of both inputs.</returns>
        public Wire And(Wire leftInput, Wire rightInput)
        {
            if (leftInput == Wire.Zero || rightInput == Wire.Zero)
                return Wire.Zero;

            if (leftInput == Wire.One)
                return rightInput;

            if (rightInput == Wire.One)
                return leftInput;
            
            return Wire.FromGate(new AndGate(RequestGateContext(ref _nextAndGateId), leftInput.Gate, rightInput.Gate));
        }

        /// <summary>
        /// Adds a binary XOR gate to the circuit.
        /// </summary>
        /// <param name="leftInput">Left input wire.</param>
        /// <param name="rightInput">Right input wire.</param>
        /// <returns>A wire representing the logical XOR of both inputs.</returns>
        public Wire Xor(Wire leftInput, Wire rightInput)
        {
            if (leftInput == Wire.Zero)
                return rightInput;

            if (rightInput == Wire.Zero)
                return leftInput;

            if (leftInput == Wire.One)
                return Not(rightInput);

            if (rightInput == Wire.One)
                return Not(leftInput);
            
            return Wire.FromGate(new XorGate(RequestGateContext(ref _nextXorGateId), leftInput.Gate, rightInput.Gate));
        }

        /// <summary>
        /// Constructs the equivalent of an OR gate from an AND gate and negated input and
        /// output wires.
        /// </summary>
        /// <param name="leftInput">Left input wire.</param>
        /// <param name="rightInput">Right input wire.</param>
        /// <returns>A wire representing the logical OR of both inputs.</returns>
        public Wire Or(Wire leftInput, Wire rightInput)
        {
            return Not(And(Not(leftInput), Not(rightInput)));
        }

        /// <summary>
        /// Adds a unary NOT gate to the circuit.
        /// </summary>
        /// <param name="input">Input wire to negate.</param>
        /// <returns>A wire representing the logical negation of the input.</returns>
        public Wire Not(Wire input)
        {
            if (input == Wire.Zero)
                return Wire.One;

            if (input == Wire.One)
                return Wire.Zero;
            
            return Wire.FromGate(new NotGate(RequestGateContext(ref _nextNotGateId), input.Gate));
        }

        /// <summary>
        /// Adds a new input wire to the circuit.
        /// </summary>
        /// <returns>Input wire.</returns>
        public Wire Input()
        {
            return Wire.FromGate(new InputGate(RequestGateContext(ref _nextInputGateId)));
        }

        /// <summary>
        /// Marks an existing wire as output of the circuit.
        /// </summary>
        /// <param name="wire">Wire to output.</param>
        public void Output(Wire wire)
        {
            if (wire == Wire.Zero || wire == Wire.One)
                throw new ArgumentException("Constant wires are not allowed as output.");

            _outputGates.Add(new OutputGate(RequestGateContext(ref _nextOutputGateId), wire.Gate));
        }

        private GateContext RequestGateContext(ref int nextTypeUniqueId)
        {
            return new GateContext(_nextGateId++, nextTypeUniqueId++);
        }

        public Circuit CreateCircuit()
        {
            CircuitContext circuitContext = new CircuitContext(
                _nextAndGateId,
                _nextXorGateId,
                _nextNotGateId,
                _nextInputGateId,
                _nextOutputGateId
            );
            
            return new Circuit(_outputGates, circuitContext);
        }
    }
}
