using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Internal
{
    public class NotGate : Gate
    {
        private int _inputId;

        public NotGate(GateContext context, int inputId)
             : base(context)
        {
            _inputId = inputId;
        }

        public override T Evaluate<T>(IBooleanCircuitEvaluator<T> evaluator, IdMapping<WireState<T>> wireStates, CircuitContext circuitContext)
        {
            return evaluator.EvaluateNotGate(wireStates[_inputId].Value, Context, circuitContext);
        }

        public override IEnumerable<int> InputWireIds
        {
            get
            {
                yield return _inputId;
            }
        }
    }
}
