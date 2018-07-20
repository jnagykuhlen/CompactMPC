using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Internal
{
    public struct WireState<T>
    {
        public static readonly WireState<T> Empty = new WireState<T>();

        private T _value;
        private bool _isEvaluated;

        public WireState(T value)
        {
            _value = value;
            _isEvaluated = true;
        }

        public void SetEvaluated(T value)
        {
            _value = value;
            _isEvaluated = true;
        }

        public T Value
        {
            get
            {
                return _value;
            }
        }

        public bool IsEvaluated
        {
            get
            {
                return _isEvaluated;
            }
        }
    }
}
