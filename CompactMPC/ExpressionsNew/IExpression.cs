using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew
{
    public interface IExpression
    {
        IReadOnlyList<Wire> Wires { get; }
    }
}