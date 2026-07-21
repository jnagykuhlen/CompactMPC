using System;
using CompactMPC.Expressions;

namespace CompactMPC.Protocol.Internal;

public class InputExpressionDescription(IExpression expression, Func<SecureProgramInput, IInputValue> inputValueFactory)
{
    public IExpression Expression { get; } = expression;
    public IInputValue GetInputValue(SecureProgramInput programInput) => inputValueFactory(programInput);
}
