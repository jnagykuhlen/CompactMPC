using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public abstract class Gate
    {
        private GateContext _context;

        public Gate(GateContext context)
        {
            _context = context;
        }
        
        public abstract void Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            EvaluationState<T> evaluationState,
            CircuitContext circuitContext
        );

        public abstract IEnumerable<Gate> InputGates { get; }

        public GateContext Context
        {
            get
            {
                return _context;
            }
        }
    }
}
