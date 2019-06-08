using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public abstract class BinaryForwardGate : ForwardGate
    {
        public override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState, CircuitContext circuitContext)
        {
            Optional<T> cachedInputValue = evaluationState.ReadInputValueFromCache(this);
            if (cachedInputValue.IsPresent)
            {
                ReceiveInputValues(cachedInputValue.Value, value, evaluator, evaluationState, circuitContext);
            }
            else
            {
                evaluationState.WriteInputValueToCache(this, value);
            }
        }

        protected abstract void ReceiveInputValues<T>(T leftValue, T rightValue, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState, CircuitContext circuitContext);
    }
}
