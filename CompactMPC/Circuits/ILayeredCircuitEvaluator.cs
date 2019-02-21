using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public interface ILayeredCircuitEvaluator<TIn, TProcess, TOut> : ICircuitEvaluator<TIn, TProcess, TOut>
    {
        void BeginLayer(int depthLayer, CircuitContext circuitContext);
        void EndLayer(int depthLayer, CircuitContext circuitContext);
    }
}
