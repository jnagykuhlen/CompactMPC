using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public class Optional<T>
    {
        public static readonly Optional<T> Empty = new Optional<T>(default(T), false);

        private T _value;
        private bool _isPresent;
        
        private Optional(T value, bool isPresent)
        {
            _value = value;
            _isPresent = isPresent;
        }
        
        public static Optional<T> FromValue(T value)
        {
            return new Optional<T>(value, true);
        }

        public static Optional<T> FromNullable(T nullable)
        {
            return new Optional<T>(nullable, nullable != null);
        }

        public T Value
        {
            get
            {
                return _value;
            }
        }

        public bool IsPresent
        {
            get
            {
                return _isPresent;
            }
        }
    }
}
