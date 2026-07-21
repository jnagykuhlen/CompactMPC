using CompactMPC.Circuits.Internal;

namespace CompactMPC.Circuits;

public class ForwardCircuitEvaluationResult<T>(ForwardEvaluationState<T> evaluationState)
{
    public T Value(Wire wire) => evaluationState.GetOutputValue(wire.Gate);

    public ForwardCircuitEvaluationResult<T> Value(Wire wire, out T value)
    {
        value = Value(wire);
        return this;
    }
}
