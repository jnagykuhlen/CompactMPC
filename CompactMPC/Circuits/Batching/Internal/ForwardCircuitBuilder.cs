using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardCircuitBuilder : ICircuitEvaluator<ForwardGate>
    {
        public static IReadOnlyList<ForwardGate> Build(IEvaluableCircuit circuit)
        {
            ForwardGate[] inputGates = new ForwardGate[circuit.Context.NumberOfInputGates];
            for (int i = 0; i < inputGates.Length; ++i)
                inputGates[i] = new ForwardInputGate(i);

            ForwardGate[] outputGates = circuit.Evaluate(new ForwardCircuitBuilder(), inputGates);
            for (int i = 0; i < outputGates.Length; ++i)
                outputGates[i].AddSuccessor(new ForwardOutputGate(i));

            return inputGates.Where(gate => gate.Successors.Any()).ToArray();
        }

        public ForwardGate EvaluateAndGate(ForwardGate leftInputGate, ForwardGate rightInputGate, GateContext gateContext, CircuitContext circuitContext)
        {
            ForwardGate gate = new ForwardAndGate(gateContext);
            leftInputGate.AddSuccessor(gate);
            rightInputGate.AddSuccessor(gate);
            return gate;
        }

        public ForwardGate EvaluateXorGate(ForwardGate leftInputGate, ForwardGate rightInputGate, GateContext gateContext, CircuitContext circuitContext)
        {
            ForwardGate gate = new ForwardXorGate(gateContext);
            leftInputGate.AddSuccessor(gate);
            rightInputGate.AddSuccessor(gate);
            return gate;
        }

        public ForwardGate EvaluateNotGate(ForwardGate inputGate, GateContext gateContext, CircuitContext circuitContext)
        {
            ForwardGate gate = new ForwardNotGate(gateContext);
            inputGate.AddSuccessor(gate);
            return gate;
        }
    }
}
