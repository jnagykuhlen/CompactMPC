namespace CompactMPC.Circuits.Batching.Internal
{
    public class GateEvaluation<T>
    {
        public ForwardGate Gate { get; }
        public GateEvaluationInput<T> Input { get; }
        
        public GateEvaluation(ForwardGate gate, GateEvaluationInput<T> input)
        {
            Gate = gate;
            Input = input;
        }
    }
}
