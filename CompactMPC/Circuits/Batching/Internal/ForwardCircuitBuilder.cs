namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardCircuitBuilder : ICircuitEvaluator<ForwardGate>
    {
        public static ForwardGate[] Build(IEvaluableCircuit circuit)
        {
            ForwardGate[] inputGates = new ForwardGate[circuit.Context.NumberOfInputGates];
            for (int i = 0; i < inputGates.Length; ++i)
                inputGates[i] = new ForwardInputGate();

            ForwardGate[] outputGates = circuit.Evaluate(new ForwardCircuitBuilder(), inputGates);
            for (int i = 0; i < outputGates.Length; ++i)
                outputGates[i].AddSuccessor(new ForwardOutputGate(i));

            return inputGates;
        }

        public ForwardGate EvaluateAndGate(ForwardGate leftInputGate, ForwardGate rightInputGate)
        {
            ForwardGate gate = new ForwardAndGate();
            leftInputGate.AddSuccessor(gate);
            rightInputGate.AddSuccessor(gate);
            return gate;
        }

        public ForwardGate EvaluateXorGate(ForwardGate leftInputGate, ForwardGate rightInputGate)
        {
            ForwardGate gate = new ForwardXorGate();
            leftInputGate.AddSuccessor(gate);
            rightInputGate.AddSuccessor(gate);
            return gate;
        }

        public ForwardGate EvaluateNotGate(ForwardGate inputGate)
        {
            ForwardGate gate = new ForwardNotGate();
            inputGate.AddSuccessor(gate);
            return gate;
        }
    }
}
