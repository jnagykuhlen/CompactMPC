namespace CompactMPC.Protocol.Expressions;

public static class Conditional
{
    public static ConditionalBuilder If(SecureBoolean condition) => new(condition);
}

public readonly struct ConditionalBuilder(SecureBoolean condition)
{
    public ConditionalBuilder<T> Then<T>(T ifTrue) where T : IMultiplexable<T> =>
        new(condition, ifTrue);
}

public readonly struct ConditionalBuilder<T>(SecureBoolean condition, T ifTrue) where T : IMultiplexable<T>
{
    public T Else(T ifFalse) => T.Multiplex(condition, ifTrue, ifFalse);
}
