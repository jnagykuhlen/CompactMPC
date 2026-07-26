using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Collections;
using CompactMPC.Expressions;

namespace CompactMPC.Protocol.Internal;

public record OutputExpressionDescription(IOutput<IExpression> Output, IExpression Expression)
{
    public IReadOnlyList<Bit> ReadBits(ListReader<Bit> reader) =>
        Expression.Wires.Select(wire => wire.ConstantValue ?? reader.Next()).ToList();

    public IEnumerable<Wire> NonConstantWires => Expression.Wires.Where(wire => !wire.IsConstant);
}
