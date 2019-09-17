using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardInputGate : ForwardGate
    {
        private int _inputIndex;

        public ForwardInputGate(int inputIndex)
        {
            _inputIndex = inputIndex;
        }
        
        public override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            SendOutputValue(value, evaluator, evaluationState);
        }
    }
}
