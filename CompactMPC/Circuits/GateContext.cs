using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class GateContext
    {
        private int _circuitUniqueId;
        private int _typeUniqueId;

        public GateContext(int circuitUniqueId, int typeUniqueId)
        {
            _circuitUniqueId = circuitUniqueId;
            _typeUniqueId = typeUniqueId;
        }

        /// <summary>
        /// Gets the identifier of the current gate that is unique for all gates in the circuit.
        /// </summary>
        public int CircuitUniqueId
        {
            get
            {
                return _circuitUniqueId;
            }
        }

        /// <summary>
        /// Gets the identifier of the current gate that is unique with respect to its gate type.
        /// </summary>
        public int TypeUniqueId
        {
            get
            {
                return _typeUniqueId;
            }
        }
    }
}
