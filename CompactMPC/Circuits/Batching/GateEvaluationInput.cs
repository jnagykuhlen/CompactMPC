using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching
{
    public class GateEvaluationInput<T>
    {
        private T _leftValue;
        private T _rightValue;

        public GateEvaluationInput(T leftValue, T rightValue)
        {
            _leftValue = leftValue;
            _rightValue = rightValue;
        }

        public T LeftValue
        {
            get
            {
                return _leftValue;
            }
        }

        public T RightValue
        {
            get
            {
                return _rightValue;
            }
        }
    }
}
