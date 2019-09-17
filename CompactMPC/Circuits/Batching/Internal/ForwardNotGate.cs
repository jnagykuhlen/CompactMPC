using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardNotGate : ForwardGate
    {
        public override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            T outputValue = evaluator.EvaluateNotGate(value);
            SendOutputValue(outputValue, evaluator, evaluationState);
        }
    }
}
