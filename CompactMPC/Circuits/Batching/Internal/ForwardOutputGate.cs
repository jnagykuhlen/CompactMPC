using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardOutputGate : ForwardGate
    {
        private int _outputIndex;

        public ForwardOutputGate(int outputIndex)
        {
            _outputIndex = outputIndex;
        }
        
        public override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            evaluationState.SetOutput(_outputIndex, value);
        }
    }
}
