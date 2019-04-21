using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching
{
    public interface IBatchedCircuitEvaluator<T>
    {
        T[] EvaluateAndGateBatch(GateEvaluationInput<T>[] evaluationInputs, CircuitContext circuitContext);
        T EvaluateXorGate(T leftValue, T rightValue, GateContext gateContext, CircuitContext circuitContext);
        T EvaluateNotGate(T value, GateContext gateContext, CircuitContext circuitContext);
    }
}
