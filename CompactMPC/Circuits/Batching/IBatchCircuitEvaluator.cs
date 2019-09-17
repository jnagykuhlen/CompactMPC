using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching
{
    public interface IBatchCircuitEvaluator<T>
    {
        T[] EvaluateAndGateBatch(GateEvaluationInput<T>[] evaluationInputs);
        T EvaluateXorGate(T leftValue, T rightValue);
        T EvaluateNotGate(T value);
    }
}
