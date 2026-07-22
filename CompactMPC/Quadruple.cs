using System;
using System.Collections;
using System.Collections.Generic;

namespace CompactMPC;

public class Quadruple<T> : IReadOnlyList<T>
{
    public const int Length = 4;

    private readonly T[] _values;

    public Quadruple()
    {
        _values = new T[Length];
    }

    public Quadruple(T v0, T v1, T v2, T v3)
    {
        _values = [v0, v1, v2, v3];
    }

    public Quadruple(T[] values)
    {
        if (values.Length != Length)
            throw new ArgumentException("Source array must contain exactly four values.", nameof(values));

        _values = (T[])values.Clone();
    }
        
    public T this[int index]
    {
        get
        {
            if (index is < 0 or >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _values[index];
        }
        set
        {
            if (index is < 0 or >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            _values[index] = value;
        }
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_values).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
    int IReadOnlyCollection<T>.Count => Length;
}
