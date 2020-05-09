using System.Collections.Generic;
using CompactMPC.Circuits;

namespace CompactMPC.Expressions
{
    public abstract class SecurePrimitive
    {
        private readonly Wire[] _wires;

        public CircuitBuilder Builder { get; }

        protected SecurePrimitive(CircuitBuilder builder, Wire[] wires)
        {
            Builder = builder;
            _wires = wires;
        }

        public IReadOnlyList<Wire> Wires
        {
            get
            {
                return _wires;
            }
        }
    }
}
