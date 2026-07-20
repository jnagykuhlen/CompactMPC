using System;
using CompactMPC.ExpressionsNew;
using CompactMPC.Protocol.New;

namespace CompactMPC.Protocol.Internal;

public class InputExpressionDescription(IExpression expression, Func<SecureProgramInput, IInputValue> inputValueFactory)
{
    public IExpression Expression { get; } = expression;
    public IInputValue GetInputValue(SecureProgramInput programInput) => inputValueFactory(programInput);
}
