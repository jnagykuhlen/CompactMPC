using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching
{
    public class GateEvaluationInput<T>
    {
        private GateContext _context;
        private T _leftValue;
        private T _rightValue;

        public GateEvaluationInput(GateContext context, T leftValue, T rightValue)
        {
            _context = context;
            _leftValue = leftValue;
            _rightValue = rightValue;
        }

        public GateContext Context
        {
            get
            {
                return _context;
            }
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
