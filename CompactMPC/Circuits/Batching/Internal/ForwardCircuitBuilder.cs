namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardCircuitBuilder : ICircuitEvaluator<ForwardGate>
    {
        public static ForwardCircuit Build(IEvaluableCircuit circuit)
        {
            ForwardGate[] inputGates = new ForwardGate[circuit.Context.NumberOfInputGates];
            for (int i = 0; i < inputGates.Length; ++i)
                inputGates[i] = new ForwardInputGate();

            ForwardGate[] outputGates = circuit.Evaluate(new ForwardCircuitBuilder(), inputGates);

            return new ForwardCircuit(inputGates, outputGates);
        }

        public ForwardGate EvaluateAndGate(ForwardGate leftInputGate, ForwardGate rightInputGate)
        {
            return new ForwardAndGate(leftInputGate, rightInputGate);
        }

        public ForwardGate EvaluateXorGate(ForwardGate leftInputGate, ForwardGate rightInputGate)
        {
            return new ForwardXorGate(leftInputGate, rightInputGate);
        }

        public ForwardGate EvaluateNotGate(ForwardGate inputGate)
        {
            return new ForwardNotGate(inputGate);
        }
    }
}
