using System;
using System.Collections.Generic;
using CompactMPC.ExpressionsNew;

namespace CompactMPC.Protocol.New;

public class SecureProgramOutput(Func<object, (IExpression, IReadOnlyList<Bit>)> selector)
{
    public TValue GetValue<TValue>(IOutput<IOutputExpression<TValue>> output)
    {
        var (expression, source) = selector(output);
        return ((IOutputExpression<TValue>)expression).ReadFrom(source);
    }

    public SecureProgramOutput GetValue<TValue>(IOutput<IOutputExpression<TValue>> output, out TValue value)
    {
        value = GetValue(output);
        return this;
    }
}

public interface IExpressionSource
{
    TExpression GetExpression<TExpression>(IOutput<TExpression> output) where TExpression : IExpression;
}
