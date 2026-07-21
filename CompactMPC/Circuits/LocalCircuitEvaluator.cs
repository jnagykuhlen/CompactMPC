namespace CompactMPC.Circuits;

public class LocalCircuitEvaluator : ICircuitEvaluator<Bit>
{
    public static readonly LocalCircuitEvaluator Instance = new();
        
    public Bit EvaluateAndGate(Bit leftValue, Bit rightValue) => leftValue && rightValue;
    public Bit EvaluateXorGate(Bit leftValue, Bit rightValue) => leftValue ^ rightValue;
    public Bit EvaluateNotGate(Bit value) => ~value;
}
