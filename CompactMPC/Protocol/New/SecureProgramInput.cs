using System.Collections.Generic;
using CompactMPC.ExpressionsNew;

namespace CompactMPC.Protocol.New;

public class SecureProgramInput
{
    private readonly Dictionary<object, IInputInfo> _inputs = new();

    public SecureProgramInput SetValue<TValue>(IInput<IInputExpression<TValue>> input, TValue value)
    {
        if (!_inputs.TryAdd(input, new InputInfo<TValue>(value)))
            throw new ProtocolException("Input has already been assigned.");

        return this;
    }

    public void WriteBits<TExpression>(IInput<TExpression> input, TExpression expression, BitArray destination, int position) where TExpression: IExpression =>
        (_inputs.GetValueOrDefault(input) ?? throw new ProtocolException("Input has not been assigned.")).WriteBits(expression, destination, position);

    private interface IInputInfo
    {
        void WriteBits(IExpression expression, BitArray destination, int position);
    }
    
    private record InputInfo<TValue>(TValue Value) : IInputInfo
    {
        public void WriteBits(IExpression expression, BitArray destination, int position) =>
            ((IInputExpression<TValue>)expression).WriteBits(Value, destination, position);
    }
}
