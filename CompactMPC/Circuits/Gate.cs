using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public abstract class Gate
    {
        private int _id;

        public Gate(int id)
        {
            _id = id;
        }
        
        public abstract void Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            EvaluationState<T> evaluationState
        );

        public abstract IEnumerable<Gate> InputGates { get; }

        public int Id
        {
            get
            {
                return _id;
            }
        }
    }
}
