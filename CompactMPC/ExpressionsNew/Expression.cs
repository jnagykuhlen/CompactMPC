using System.Collections.Generic;
using CompactMPC.Circuits.New;

namespace CompactMPC.ExpressionsNew
{
    public abstract class Expression : IExpression
    {
        public IReadOnlyList<Wire> Wires { get; }

        protected Expression(IReadOnlyList<Wire> wires)
        {
            Wires = wires;
        }
    }
}
