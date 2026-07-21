using CompactMPC.Collections;

namespace CompactMPC.Expressions;

public interface IInputExpression<in T> : IExpression
{
    void WriteTo(T value, IWriteOnlyList<Bit> destination);
}
