using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits.Internal;

namespace CompactMPC.Circuits
{
    public sealed class Wire
    {
        public static readonly Wire Zero = new Wire(null);
        public static readonly Wire One = new Wire(null);

        private Gate _gate;

        private Wire(Gate gate)
        {
            _gate = gate;
        }

        public static Wire CreateAssignable()
        {
            return new Wire(null);
        }

        public static Wire And(Wire leftInput, Wire rightInput)
        {
            if (leftInput == Zero || rightInput == Zero)
                return Zero;

            if (leftInput == One)
                return rightInput;

            if (rightInput == One)
                return leftInput;

            return new Wire(new AndGate(leftInput, rightInput));
        }

        public static Wire Xor(Wire leftInput, Wire rightInput)
        {
            if (leftInput == Zero)
                return rightInput;

            if (rightInput == Zero)
                return leftInput;

            if (leftInput == One)
                return Not(rightInput);

            if (rightInput == One)
                return Not(leftInput);

            return new Wire(new XorGate(leftInput, rightInput));
        }

        public static Wire Not(Wire input)
        {
            if (input == Zero)
                return One;

            if (input == One)
                return Zero;

            return new Wire(new NotGate(input));
        }

        public static Wire Or(Wire leftInput, Wire rightInput)
        {
            return Not(And(Not(leftInput), Not(rightInput)));
        }

        public Gate Gate
        {
            get
            {
                return _gate;
            }
        }

        public bool IsAssignable
        {
            get
            {
                return !IsConstant && _gate == null;
            }
        }

        public bool IsConstant
        {
            get
            {
                return this == Zero || this == One;
            }
        }
    }
}
