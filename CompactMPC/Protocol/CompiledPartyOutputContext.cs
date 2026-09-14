using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol;

public class CompiledPartyOutputContext(IReadOnlyList<OutputExpressionDescription> expressionDescriptions)
{
    public IReadOnlyList<OutputExpressionDescription> ExpressionDescriptions { get; } = expressionDescriptions;

    public int TotalNumberOfNonConstantBits { get; } =
        expressionDescriptions.Sum(description => description.NonConstantWires.Count());

    public IEnumerable<Wire> NonConstantWires =>
        ExpressionDescriptions.SelectMany(description => description.NonConstantWires);
}
