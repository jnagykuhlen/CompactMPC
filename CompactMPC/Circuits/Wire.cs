using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class Wire
    {
        public static readonly Wire Zero = new Wire(null);
        public static readonly Wire One = new Wire(null);

        private Gate _gate;

        private Wire(Gate gate)
        {
            _gate = gate;
        }

        public static Wire FromGate(Gate gate)
        {
            if (gate == null)
                throw new ArgumentNullException(nameof(gate));
            
            return new Wire(gate);
        }

        public Gate Gate
        {
            get
            {
                return _gate;
            }
        }

        /*
        public bool IsConstant
        {
            get
            {
                return _id < 0;
            }
        }
        */
    }
}
