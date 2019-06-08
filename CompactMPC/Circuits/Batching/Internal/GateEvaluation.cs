using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class GateEvaluation<T>
    {
        private ForwardGate _gate;
        private GateEvaluationInput<T> _input;

        public GateEvaluation(ForwardGate gate, GateEvaluationInput<T> input)
        {
            _gate = gate;
            _input = input;
        }

        public ForwardGate Gate
        {
            get
            {
                return _gate;
            }
        }

        public GateEvaluationInput<T> Input
        {
            get
            {
                return _input;
            }
        }
    }
}
