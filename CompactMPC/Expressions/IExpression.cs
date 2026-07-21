using System.Collections.Generic;
using CompactMPC.Circuits;

namespace CompactMPC.Expressions;

public interface IExpression
{
    IReadOnlyList<Wire> Wires { get; }
}
