using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew.Local
{
    public interface IInputBinding
    {
        IReadOnlyList<Wire> Wires { get; }
        IReadOnlyList<Bit> Bits { get; }
    }
}