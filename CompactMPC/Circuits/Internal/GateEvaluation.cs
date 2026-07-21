namespace CompactMPC.Circuits.Internal;

public record GateEvaluation<T>(ForwardGate Gate, GateEvaluationInput<T> Input);
