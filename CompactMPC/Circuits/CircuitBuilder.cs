using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Expressions;

namespace CompactMPC.Circuits
{
    /// <summary>
    /// Represents an abstract builder for constructing boolean circuits ad hoc from
    /// AND, XOR and NOT gates. Derived classes need to translate these gates into
    /// an appropriate internal circuit representation.
    /// </summary>
    public abstract class CircuitBuilder
    {
        private int _nextWireId;
        private int _nextGateId;
        private int _nextAndGateId;
        private int _nextXorGateId;
        private int _nextNotGateId;
        private CircuitContext _cachedContext;

        /// <summary>
        /// Creates a new empty circuit builder.
        /// </summary>
        protected CircuitBuilder()
        {
            _nextWireId = 0;
            _nextGateId = 0;
            _nextAndGateId = 0;
            _nextXorGateId = 0;
            _nextNotGateId = 0;
            _cachedContext = null;
        }

        /// <summary>
        /// Adds a binary AND gate to the circuit.
        /// </summary>
        /// <param name="leftInput">Left input wire.</param>
        /// <param name="rightInput">Right input wire.</param>
        /// <returns>A wire representing the logical AND of both inputs.</returns>
        public Wire And(Wire leftInput, Wire rightInput)
        {
            if (!IsValid(leftInput) || !IsValid(rightInput))
                throw new ArgumentException("Invalid secure bit identifier.");

            if (leftInput == Wire.Zero || rightInput == Wire.Zero)
                return Wire.Zero;

            if (leftInput == Wire.One)
                return rightInput;

            if (rightInput == Wire.One)
                return leftInput;

            Wire output = RequestWire();
            AddAndGate(leftInput, rightInput, output, RequestGate(ref _nextAndGateId));
            return output;
        }

        /// <summary>
        /// Adds a binary XOR gate to the circuit.
        /// </summary>
        /// <param name="leftInput">Left input wire.</param>
        /// <param name="rightInput">Right input wire.</param>
        /// <returns>A wire representing the logical XOR of both inputs.</returns>
        public Wire Xor(Wire leftInput, Wire rightInput)
        {
            if (!IsValid(leftInput) || !IsValid(rightInput))
                throw new ArgumentException("Invalid secure bit identifier.");

            if (leftInput == Wire.Zero)
                return rightInput;

            if (rightInput == Wire.Zero)
                return leftInput;

            if (leftInput == Wire.One)
                return Not(rightInput);

            if (rightInput == Wire.One)
                return Not(leftInput);

            Wire output = RequestWire();
            AddXorGate(leftInput, rightInput, output, RequestGate(ref _nextXorGateId));
            return output;
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
            if (!IsValid(input))
                throw new ArgumentException("Invalid secure bit identifier.");

            if (input == Wire.Zero)
                return Wire.One;

            if (input == Wire.One)
                return Wire.Zero;

            Wire result = RequestWire();
            AddNotGate(input, result, RequestGate(ref _nextNotGateId));
            return result;
        }

        /// <summary>
        /// Adds a new input wire to the circuit.
        /// </summary>
        /// <returns>Input wire.</returns>
        public Wire Input()
        {
            Wire wire = RequestWire();
            MakeInputWire(wire);
            return wire;
        }

        /// <summary>
        /// Marks an existing wire as output of the circuit.
        /// </summary>
        /// <param name="wire">Wire to output.</param>
        public void Output(Wire wire)
        {
            if (wire.IsConstant)
                throw new ArgumentException("Constant wires are not allow as output.");

            if (!IsValid(wire))
                throw new ArgumentException("Invalid secure bit identifier.");
            
            MakeOutputWire(wire);
        }

        private GateContext RequestGate(ref int nextTypeUniqueId)
        {
            _cachedContext = null;
            return new GateContext(_nextGateId++, nextTypeUniqueId++);
        }

        private Wire RequestWire()
        {
            return Wire.FromId(_nextWireId++);
        }

        private bool IsValid(Wire bit)
        {
            return (bit.Id >= 0 && bit.Id < _nextWireId) || bit.IsConstant;
        }

        protected abstract void AddAndGate(Wire leftInput, Wire rightInput, Wire output, GateContext context);
        protected abstract void AddXorGate(Wire leftInput, Wire rightInput, Wire output, GateContext context);
        protected abstract void AddNotGate(Wire input, Wire output, GateContext context);
        protected abstract void MakeInputWire(Wire bit);
        protected abstract void MakeOutputWire(Wire bit);
        
        /// <summary>
        /// Gets information on the number of gates in the circuit.
        /// </summary>
        public CircuitContext CircuitContext
        {
            get
            {
                if (_cachedContext == null)
                {
                    _cachedContext = new CircuitContext(
                        _nextAndGateId,
                        _nextXorGateId,
                        _nextNotGateId
                    );
                }
                
                return _cachedContext;
            }
        }
    }
}
