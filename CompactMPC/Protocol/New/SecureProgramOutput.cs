using System.Collections.Generic;
using CompactMPC.ExpressionsNew;

namespace CompactMPC.Protocol.New;

public class SecureProgramOutput(IReadOnlyDictionary<IOutput<IExpression>, (IExpression Expression, IReadOnlyList<Bit> Bits)> outputValues)
{
    public TValue GetValue<TValue>(IOutput<IOutputExpression<TValue>> output)
    {
        var (expression, bits) = outputValues[output];
        return ((IOutputExpression<TValue>)expression).ReadFrom(bits);
    }

    public SecureProgramOutput GetValue<TValue>(IOutput<IOutputExpression<TValue>> output, out TValue value)
    {
        value = GetValue(output);
        return this;
    }
}
