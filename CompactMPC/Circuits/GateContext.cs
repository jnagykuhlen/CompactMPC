using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    /// <summary>
    /// Combines information on the identity of a gate in a boolean circuit.
    /// </summary>
    public class GateContext
    {
        private int _circuitUniqueId;
        private int _typeUniqueId;

        /// <summary>
        /// Creates a new set of information on a gate in a circuit.
        /// </summary>
        /// <param name="circuitUniqueId">An identifier of the current gate that is unique for all gates in the circuit.</param>
        /// <param name="typeUniqueId">An identifier of the current gate that is unique with respect to its gate type.</param>
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
