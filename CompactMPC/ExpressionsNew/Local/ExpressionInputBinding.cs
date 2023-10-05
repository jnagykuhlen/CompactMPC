using System;
using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits.New;

namespace CompactMPC.ExpressionsNew.Local
{
    public class ExpressionInputBinding
    {
        public static ExpressionInputBinding From<T>(IInputExpression<T> expression, T value)
        {
            List<WireValue<Bit>> wireValues = expression.Wires
                .Zip(expression.ToBits(value), WireValue.Create)
                .ToList();
            
            return new ExpressionInputBinding(wireValues);
        }

        private ExpressionInputBinding(IReadOnlyList<WireValue<Bit>> wireValues)
        {
            WireValues = wireValues;
        }

        public IReadOnlyList<WireValue<Bit>> WireValues { get; }
    }
}