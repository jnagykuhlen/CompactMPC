using System;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.ExpressionsNew.Local
{
    public class InputBinding
    {
        public static InputBinding From<T>(IInputExpression<T> expression, T value)
        {
            if (expression.Wires.Any(wire => wire.IsConstant))
                throw new ArgumentException("Cannot create input binding for constant expression.", nameof(expression));

            return new InputBinding(expression.Wires, expression.ToBits(value));
        }

        private InputBinding(IReadOnlyList<Wire> wires, IReadOnlyList<Bit> bits)
        {
            Wires = wires;
            Bits = bits;
        }

        public IReadOnlyList<Wire> Wires { get; }

        public IReadOnlyList<Bit> Bits { get; }
    }
}