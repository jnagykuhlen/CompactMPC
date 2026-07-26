using System.Collections.Generic;
using CompactMPC.Circuits;

namespace CompactMPC.Protocol;

public interface IExpression
{
    IReadOnlyList<Wire> Wires { get; }
}
