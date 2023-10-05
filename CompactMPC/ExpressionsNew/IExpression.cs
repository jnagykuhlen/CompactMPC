using System.Collections.Generic;
using CompactMPC.Circuits.New;

namespace CompactMPC.ExpressionsNew
{
    public interface IExpression
    {
        IReadOnlyList<Wire> Wires { get; }
    }
}