using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;

namespace CompactMPC.Protocol.Internal;

public class PartyOutputContext
{
    private readonly List<OutputExpressionDescription> _expressionDescriptions = new();

    public int TotalNumberOfNonConstantBits { get; private set; }

    public void Add<TExpression>(IOutput<TExpression> output, TExpression expression) where TExpression : IExpression
    {
        var expressionDescription = new OutputExpressionDescription((IOutput<IExpression>)output, expression);
        TotalNumberOfNonConstantBits += expressionDescription.NonConstantWires.Count();
        _expressionDescriptions.Add(expressionDescription);
    }

    public IReadOnlyList<OutputExpressionDescription> ExpressionDescriptions => _expressionDescriptions;

    public IEnumerable<Wire> NonConstantWires =>
        ExpressionDescriptions.SelectMany(description => description.NonConstantWires);
}
