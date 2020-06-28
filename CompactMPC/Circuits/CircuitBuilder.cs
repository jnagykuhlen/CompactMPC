using System;
using System.Collections.Generic;
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
        private int _numberOfAndGates;
        private int _numberOfXorGates;
        private int _numberOfNotGates;
        private int _nextInputIndex;
        private int _nextOutputIndex;
        private readonly List<OutputGate> _outputGates;

        /// <summary>
        /// Creates a new empty circuit builder.
        /// </summary>
        public CircuitBuilder()
        {
            _nextGateId = 0;
            _numberOfAndGates = 0;
            _numberOfXorGates = 0;
            _numberOfNotGates = 0;
            _nextInputIndex = 0;
            _nextOutputIndex = 0;
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

            _numberOfAndGates++;
            
            return Wire.FromGate(new AndGate(_nextGateId++, leftInput.Gate, rightInput.Gate));
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

            _numberOfXorGates++;
            
            return Wire.FromGate(new XorGate(_nextGateId++, leftInput.Gate, rightInput.Gate));
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

            _numberOfNotGates++;
            
            return Wire.FromGate(new NotGate(_nextGateId++, input.Gate));
        }

        /// <summary>
        /// Adds a new input wire to the circuit.
        /// </summary>
        /// <returns>Input wire.</returns>
        public Wire Input()
        {
            return Wire.FromGate(new InputGate(_nextGateId++, _nextInputIndex++));
        }

        /// <summary>
        /// Marks an existing wire as output of the circuit.
        /// </summary>
        /// <param name="wire">LegacyWire to output.</param>
        public void Output(Wire wire)
        {
            if (wire == Wire.Zero || wire == Wire.One)
                throw new ArgumentException("Constant wires are not allowed as output.");

            _outputGates.Add(new OutputGate(_nextGateId++, wire.Gate, _nextOutputIndex++));
        }

        public Circuit CreateCircuit()
        {
            CircuitContext circuitContext = new CircuitContext(
                _numberOfAndGates,
                _numberOfXorGates,
                _numberOfNotGates,
                _nextInputIndex,
                _nextOutputIndex
            );
            
            return new Circuit(_outputGates, circuitContext);
        }
    }
}
