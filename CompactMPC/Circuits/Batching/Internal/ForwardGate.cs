using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public abstract class ForwardGate
    {
        private List<ForwardGate> _successors;

        public ForwardGate()
        {
            _successors = new List<ForwardGate>();
        }

        public abstract void Evaluate<T>(IBatchedCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState, CircuitContext circuitContext);
        public abstract int NumberOfInputs { get; }
        
        public void AddSuccessor(ForwardGate sucessor)
        {
            _successors.Add(sucessor);
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
