namespace CompactMPC.Circuits.Batching
{
    public interface IBatchEvaluableCircuit
    {
        T[] Evaluate<T>(IBatchCircuitEvaluator<T> evaluator, T[] inputValues);
        CircuitContext Context { get; }
    }
}
