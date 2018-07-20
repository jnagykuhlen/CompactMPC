using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Internal
{
    public class AndGate : Gate
    {
        private int _leftInputId;
        private int _rightInputId;

        public AndGate(GateContext context, int leftInputId, int rightInputId)
             : base(context)
        {
            _leftInputId = leftInputId;
            _rightInputId = rightInputId;
        }
        
        public override T Evaluate<T>(IBooleanCircuitEvaluator<T> evaluator, IdMapping<WireState<T>> wireStates, CircuitContext circuitContext)
        {
            return evaluator.EvaluateAndGate(wireStates[_leftInputId].Value, wireStates[_rightInputId].Value, Context, circuitContext);
        }

        public override IEnumerable<int> InputWireIds
        {
            get
            {
                yield return _leftInputId;
                yield return _rightInputId;
            }
        }
    }
}
