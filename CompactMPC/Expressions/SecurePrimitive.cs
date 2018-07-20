using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;

namespace CompactMPC.Expressions
{
    public abstract class SecurePrimitive
    {
        private CircuitBuilder _builder;
        private Wire[] _wires;

        protected SecurePrimitive(CircuitBuilder builder, Wire[] wires)
        {
            _builder = builder;
            _wires = wires;
        }
        
        public CircuitBuilder Builder
        {
            get
            {
                return _builder;
            }
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
