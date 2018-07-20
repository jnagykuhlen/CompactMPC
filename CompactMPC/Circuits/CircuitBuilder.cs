using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Expressions;

namespace CompactMPC.Circuits
{
    public abstract class CircuitBuilder
    {
        private int _nextWireId;
        private int _nextGateId;
        private int _nextAndGateId;
        private int _nextXorGateId;
        private int _nextNotGateId;
        private CircuitContext _cachedContext;

        public CircuitBuilder()
        {
            _nextWireId = 0;
            _nextGateId = 0;
            _nextAndGateId = 0;
            _nextXorGateId = 0;
            _nextNotGateId = 0;
            _cachedContext = null;
        }

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

        public Wire Or(Wire leftInput, Wire rightInput)
        {
            return Not(And(Not(leftInput), Not(rightInput)));
        }

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

        public Wire Input()
        {
            Wire wire = RequestWire();
            MakeInputWire(wire);
            return wire;
        }

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
        
        public int NumberOfGates
        {
            get
            {
                return _nextGateId;
            }
        }

        public int NumberOfAndGates
        {
            get
            {
                return _nextAndGateId;
            }
        }

        public int NumberOfXorGates
        {
            get
            {
                return _nextXorGateId;
            }
        }

        public int NumberOfNotGates
        {
            get
            {
                return _nextNotGateId;
            }
        }

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
