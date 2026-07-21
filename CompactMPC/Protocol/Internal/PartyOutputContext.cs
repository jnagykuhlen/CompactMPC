using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Expressions;

namespace CompactMPC.Protocol.Internal;

public class PartyOutputContext
{
    private readonly List<OutputExpressionDescription> _expressionDescriptions = new();

    public int TotalNumberOfBits { get; private set; }

    public void Add<TExpression>(Output<TExpression> output, TExpression expression) where TExpression : IExpression
    {
        TotalNumberOfBits += expression.Wires.Count;
        _expressionDescriptions.Add(new OutputExpressionDescription((IOutput<IExpression>)output, expression));
    }

    public IReadOnlyList<OutputExpressionDescription> ExpressionDescriptions => _expressionDescriptions;
    public IEnumerable<Wire> Wires => ExpressionDescriptions.SelectMany(description => description.Expression.Wires);
}
