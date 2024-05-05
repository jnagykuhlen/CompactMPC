using System.Collections.Generic;
using CompactMPC.Circuits.New;

namespace CompactMPC.ExpressionsNew
{
    public abstract class Expression(IReadOnlyList<Wire> wires) : IExpression
    {
        public IReadOnlyList<Wire> Wires { get; } = wires;
    }
}
