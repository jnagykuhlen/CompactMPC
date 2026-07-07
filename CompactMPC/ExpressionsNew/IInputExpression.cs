using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew
{
    public interface IInputExpression<in T> : IExpression
    {
        void WriteBits(T value, BitArray destination, int position);
    }
}