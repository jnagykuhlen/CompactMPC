using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Internal
{
    public abstract class Gate
    {
        private GateContext _context;

        public Gate(GateContext context)
        {
            _context = context;
        }

        public abstract T Evaluate<T>(IBooleanCircuitEvaluator<T> evaluator, IdMapping<WireState<T>> wireStates, CircuitContext circuitContext);
        public abstract IEnumerable<int> InputWireIds { get; }

        public GateContext Context
        {
            get
            {
                return _context;
            }
        }
    }
}
