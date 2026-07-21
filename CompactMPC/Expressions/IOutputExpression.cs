using System.Collections.Generic;

namespace CompactMPC.Expressions;

public interface IOutputExpression<out T> : IExpression
{
    T ReadFrom(IReadOnlyList<Bit> source);
}
