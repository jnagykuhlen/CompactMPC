using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew
{
    public interface IInputBinding
    {
        IReadOnlyList<Wire> Wires { get; }
        IReadOnlyList<Bit> Bits { get; }
    }
}