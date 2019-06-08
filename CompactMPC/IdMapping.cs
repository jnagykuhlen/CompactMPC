using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public class IdMapping<T>
    {
        private const int DefaultInitialCapacity = 4;

        private T _defaultValue;
        private T[] _values;

        public IdMapping()
            : this(default(T), DefaultInitialCapacity) { }

        public IdMapping(T defaultValue)
            : this(defaultValue, DefaultInitialCapacity) { }

        public IdMapping(T defaultValue, int initialCapacity)
        {
            _defaultValue = defaultValue;
            _values = new T[initialCapacity];
            for (int i = 0; i < _values.Length; ++i)
                _values[i] = defaultValue;
        }

        public T this[int id]
        {
            get
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException(nameof(id));

                if (id < _values.Length)
                    return _values[id];

                return _defaultValue;
            }
            set
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException(nameof(id));

                int currentLength = _values.Length;
                int requiredLength = id + 1;
                if(requiredLength > currentLength)
                {
                    int newLength = currentLength;
                    while (newLength < requiredLength)
                        newLength = 2 * newLength;

                    Array.Resize(ref _values, newLength);
                    for (int i = currentLength; i < newLength; ++i)
                        _values[i] = _defaultValue;
                }

                _values[id] = value;
            }
        }
    }
}
