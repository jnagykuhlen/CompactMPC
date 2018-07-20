using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public class Quadruple<T> : IEnumerable<T>
    {
        public const int Length = 4;

        private T[] _values;

        public Quadruple()
        {
            _values = new T[Length];
        }

        public Quadruple(T v0, T v1, T v2, T v3)
        {
            _values = new T[] { v0, v1, v2, v3 };
        }

        public Quadruple(T[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (values.Length != Length)
                throw new ArgumentException("Source array must contain exactly four values.", nameof(values));

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
    }
}
