using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public abstract class Gate
    {
        public abstract T Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            CircuitEvaluation<T> evaluationState
        );
    }
}
