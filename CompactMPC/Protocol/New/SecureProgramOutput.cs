using System;
using CompactMPC.ExpressionsNew;

namespace CompactMPC.Protocol.New;

public class SecureProgramOutput
{
    public T Value<T>(IOutput<IOutputExpression<T>> output) => throw new NotImplementedException();

    public SecureProgramOutput Value<T>(IOutput<IOutputExpression<T>> output, out T value)
    {
        value = Value(output);
        return this;
    }
}
