using System.Collections.Generic;
using CompactMPC.ExpressionsNew;

namespace CompactMPC.Protocol.New;

public class SecureProgramInput
{
    private readonly Dictionary<object, object> _inputs = new();

    public SecureProgramInput SetValue<T>(IInput<IInputExpression<T>> input, T value) where T : notnull
    {
        if (!_inputs.TryAdd(input, value))
            throw new ProtocolException("Input has already been assigned.");

        return this;
    }
    
    public T Value<T>(IInput<IInputExpression<T>> input) where T : notnull =>
        (T)(_inputs.GetValueOrDefault(input) ?? throw new ProtocolException("Input has not been assigned."));
}
