using System;
using System.Collections.Generic;
using CompactMPC.Collections;
using CompactMPC.Expressions;

namespace CompactMPC.Protocol;

public class SecureProgramInput
{
    private readonly Dictionary<object, Func<IExpression, IInputValue>> _inputValueFactoriesByInput = new();

    public SecureProgramInput SetValue<TValue>(IInput<IInputExpression<TValue>> input, TValue value)
    {
        if (!_inputValueFactoriesByInput.TryAdd(input, expression => new InputValue<TValue>((IInputExpression<TValue>)expression, value)))
            throw new ProtocolException("Input has already been assigned.");

        return this;
    }

    public IInputValue GetValue<TExpression>(IInput<TExpression> input, TExpression expression) where TExpression : IExpression
    {
        var inputValueFactory =
            _inputValueFactoriesByInput.GetValueOrDefault(input) ?? throw new ProtocolException("Input has not been assigned.");

        return inputValueFactory(expression);
    }
    
    private class InputValue<TValue>(IInputExpression<TValue> expression, TValue value) : IInputValue
    {
        public void WriteTo(IWriteOnlyList<Bit> destination) => expression.WriteTo(value, destination);
    }
}

public interface IInputValue
{
    void WriteTo(IWriteOnlyList<Bit> destination);
}
