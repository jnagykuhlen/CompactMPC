using System;
using System.Collections;
using System.Collections.Generic;

namespace CompactMPC.Collections;

public static class ReadOnlyListExtensions
{
    public static IReadOnlyList<T> ReadOnlySlice<T>(this IReadOnlyList<T> list, int start, int count) =>
        new ReadOnlyListSlice<T>(list, start, count);
    
    public static ReadOnlyListReader<T> GetReader<T>(this IReadOnlyList<T> list) => new(list);

    private class ReadOnlyListSlice<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _list;
        private readonly int _start;

        public ReadOnlyListSlice(IReadOnlyList<T> list, int start, int count)
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
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _list[_start + index];
            }
        }

        public int Count { get; }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return _list[_start + i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
