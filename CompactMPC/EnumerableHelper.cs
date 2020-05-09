using System.Collections.Generic;
using System.Linq;

namespace CompactMPC
{
    public static class EnumerableHelper
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.SelectMany(item => item);
        }
    }
}
