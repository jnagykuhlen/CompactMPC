using System.Collections.Generic;
using CompactMPC.Collections;
using CompactMPC.Expressions;

namespace CompactMPC.Protocol.Internal;

public class PartyInputContext
{
    private readonly List<InputExpressionDescription> _expressionDescriptions = new();

    public int TotalNumberOfBits { get; private set; }

    public void Add<TExpression>(Input<TExpression> input, TExpression expression) where TExpression : IExpression
    {
        TotalNumberOfBits += expression.Wires.Count;
        _expressionDescriptions.Add(
            new InputExpressionDescription(expression, programInput => programInput.GetValue(input, expression))
        );
    }

    public IReadOnlyList<InputExpressionDescription> ExpressionDescriptions => _expressionDescriptions;

    public BitArray GetInputBits(SecureProgramInput programInput)
    {
        var bits = new BitArray(TotalNumberOfBits);
        var bitsWriter = bits.GetWriter();

        foreach (var description in ExpressionDescriptions)
            description.GetInputValue(programInput).WriteTo(bitsWriter.NextSlice(description.Expression.Wires.Count));

        return bits;
    }
}
