using CompactMPC.Collections;

namespace CompactMPC.Protocol;

public interface IInputExpression<in T> : IExpression
{
    void WriteTo(T value, IWriteOnlyList<Bit> destination);
}
