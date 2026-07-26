using System.Collections.Generic;

namespace CompactMPC.Protocol;

public interface IOutputExpression<out T> : IExpression
{
    T ReadFrom(IReadOnlyList<Bit> source);
}
