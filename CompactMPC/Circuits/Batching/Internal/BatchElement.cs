using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class BatchElement<T>
    {
        private GateEvaluationInput<T> _evaluationInput;
        private IReadOnlyList<ForwardGate> _successors;

        public BatchElement(GateEvaluationInput<T> evaluationInput, IReadOnlyList<ForwardGate> successors)
        {
            _evaluationInput = evaluationInput;
            _successors = successors;
        }

        public GateEvaluationInput<T> EvaluationInput
        {
            get
            {
                return _evaluationInput;
            }
        }

        public IReadOnlyList<ForwardGate> Successors
        {
            get
            {
                return _successors;
            }
        }
    }
}
