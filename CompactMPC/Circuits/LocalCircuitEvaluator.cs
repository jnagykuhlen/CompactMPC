namespace CompactMPC.Circuits
{
    public class LocalCircuitEvaluator : ICircuitEvaluator<Bit>
    {
        public Bit EvaluateAndGate(Bit leftValue, Bit rightValue)
        {
            return leftValue && rightValue;
        }

        public Bit EvaluateXorGate(Bit leftValue, Bit rightValue)
        {
            return leftValue ^ rightValue;
        }

        public Bit EvaluateNotGate(Bit value)
        {
            return ~value;
        }
    }
}
