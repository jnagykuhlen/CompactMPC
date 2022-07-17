using System.Collections.Generic;

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
