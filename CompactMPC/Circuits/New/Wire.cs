using System;
using CompactMPC.Circuits.Batching;
using CompactMPC.Circuits.Batching.Internal;

namespace CompactMPC.Circuits.New
{
    public sealed class Wire
    {
        public static readonly Wire Zero = new Wire(null);
        public static readonly Wire One = new Wire(null);

        private readonly ForwardGate? _gate;

        private Wire(ForwardGate? gate)
        {
            _gate = gate;
        }

        public static Wire And(Wire leftWire, Wire rightWire)
        {
            if (leftWire == Zero || rightWire == Zero)
                return Zero;

            if (leftWire == One || leftWire == rightWire)
                return rightWire;

            if (rightWire == One)
                return leftWire;

            return new Wire(new ForwardAndGate(leftWire.Gate, rightWire.Gate));
        }

        public static Wire Xor(Wire leftWire, Wire rightWire)
        {
            if (leftWire == Zero)
                return rightWire;

            if (rightWire == Zero)
                return leftWire;

            if (leftWire == One)
                return Not(rightWire);

            if (rightWire == One)
                return Not(leftWire);

            if (leftWire == rightWire)
                return Zero;

            return new Wire(new ForwardXorGate(leftWire.Gate, rightWire.Gate));
        }

        public static Wire Or(Wire leftWire, Wire rightWire)
        {
            if (leftWire == Zero || leftWire == rightWire)
                return rightWire;

            if (rightWire == Zero)
                return leftWire;

            if (leftWire == One || rightWire == One)
                return One;

            return Not(And(Not(leftWire), Not(rightWire)));
        }

        public static Wire Not(Wire wire)
        {
            if (wire == Zero)
                return One;

            if (wire == One)
                return Zero;

            return new Wire(new ForwardNotGate(wire.Gate));
        }

        public static Wire Input()
        {
            return new Wire(new ForwardInputGate());
        }

        public ForwardGate Gate => _gate ?? throw new NotSupportedException("Constant wires are not associated with a gate.");

        public bool IsConstant => _gate == null;

        public Bit? ConstantValue
        {
            get
            {
                if (this == Zero)
                    return Bit.Zero;

                if (this == One)
                    return Bit.One;

                return null;
            }
        }
    }
}
