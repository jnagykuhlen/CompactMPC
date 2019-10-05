using System;
using System.Collections.Generic;
using System.Text;

using CompactMPC.Circuits;

namespace CompactMPC.Expressions
{
    public abstract class SecurePrimitive
    {
        public abstract IReadOnlyList<Wire> Wires { get; }
    }
}
