using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using Wire = CompactMPC.Circuits.New.Wire;

namespace CompactMPC.ExpressionsNew;

public class ExpressionEvaluationResult(IReadOnlyDictionary<Wire, Bit> valuesByWire)
{
    public T Value<T>(IOutputExpression<T> expression)
    {
        var bits = expression.Wires
            .Select(wire => wire.ConstantValue ?? GetValue(wire))
            .ToList();

        return expression.ReadValue(bits, 0);
    }

    public ExpressionEvaluationResult Value<T>(IOutputExpression<T> expression, out T value)
    {
        value = Value(expression);
        return this;
    }

    private Bit GetValue(Wire wire)
    {
        if (valuesByWire.TryGetValue(wire, out var value))
            return value;
            
        throw new CircuitEvaluationException("Expression value was not evaluated.");
    }
}
