using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public class Pair<T> : IReadOnlyList<T>
    {
        public const int Length = 2;

        private T[] _values;

        public Pair()
        {
            _values = new T[Length];
        }

        public Pair(T v0, T v1 )
        {
            _values = new T[] { v0, v1 };
        }

        public Pair(T[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (values.Length != Length)
                throw new ArgumentException("Source array must contain exactly two values.", nameof(values));

            _values = (T[])values.Clone();
        }
        
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _values[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                _values[index] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        int IReadOnlyCollection<T>.Count
        {
            get
            {
                return Length;
            }
        }
    }
}
