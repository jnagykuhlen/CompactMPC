using System.Collections.Generic;
using CompactMPC.Circuits;

namespace CompactMPC.Protocol.Expressions;

public abstract class Expression(IReadOnlyList<Wire> wires) : IExpression
{
    public IReadOnlyList<Wire> Wires { get; } = wires;
}
