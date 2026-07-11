using System;

namespace CompactMPC.Collections;

public static class WriteOnlyListExtensions
{
    public static IWriteOnlyList<T> WriteOnlySlice<T>(this IWriteOnlyList<T> list, int start, int count) =>
        new WriteOnlyListSlice<T>(list, start, count);
    
    public static ListWriter<T> GetWriter<T>(this IWriteOnlyList<T> list) => new(list);

    private class WriteOnlyListSlice<T> : IWriteOnlyList<T>
    {
        private readonly IWriteOnlyList<T> _list;
        private readonly int _start;

        public WriteOnlyListSlice(IWriteOnlyList<T> list, int start, int count)
        {
            if (start < 0 || start >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(start));

            if (count < 0 || start + count > list.Count)
                throw new ArgumentOutOfRangeException(nameof(count));

            _list = list;
            _start = start;
            Count = count;
        }

        public T this[int index]
        {
            set
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                _list[_start + index] = value;
            }
        }

        public int Count { get; }
    }
}
