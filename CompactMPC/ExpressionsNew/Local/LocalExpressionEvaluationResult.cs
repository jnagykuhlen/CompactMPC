using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using Wire = CompactMPC.Circuits.New.Wire;

namespace CompactMPC.ExpressionsNew.Local
{
    public class LocalExpressionEvaluationResult
    {
        private readonly IReadOnlyDictionary<Wire, Bit> _valuesByWire;

        public LocalExpressionEvaluationResult(IReadOnlyDictionary<Wire, Bit> valuesByWire)
        {
            _valuesByWire = valuesByWire;
        }

        public T Value<T>(IOutputExpression<T> expression)
        {
            Bit GetValue(Wire wire)
            {
                if (_valuesByWire.TryGetValue(wire, out Bit value))
                    return value;
                throw new CircuitEvaluationException("Expression value was not evaluated.");
            }

            IReadOnlyList<Bit> bits = expression.Wires
                .Select(wire => wire.ConstantValue ?? GetValue(wire))
                .ToList();

            return expression.FromBits(bits);
        }
        
        public LocalExpressionEvaluationResult Value<T>(IOutputExpression<T> expression, out T value)
        {
            value = Value(expression);
            return this;
        }
    }
}