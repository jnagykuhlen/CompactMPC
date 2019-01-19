using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    /// <summary>
    /// Combines information on the number of gates in a boolean circuit.
    /// </summary>
    public class CircuitContext
    {
        private int _numberOfAndGates;
        private int _numberOfXorGates;
        private int _numberOfNotGates;

        /// <summary>
        /// Creates a new set of information on a boolean circuit.
        /// </summary>
        /// <param name="numberOfAndGates">Number of AND gates in the circuit.</param>
        /// <param name="numberOfXorGates">Number of XOR gates in the circuit.</param>
        /// <param name="numberOfNotGates">Number of NOT gates in the circuit.</param>
        public CircuitContext(int numberOfAndGates, int numberOfXorGates, int numberOfNotGates)
        {
            _numberOfAndGates = numberOfAndGates;
            _numberOfXorGates = numberOfXorGates;
            _numberOfNotGates = numberOfNotGates;
        }

        /// <summary>
        /// Gets the number of AND gates in the circuit.
        /// </summary>
        public int NumberOfAndGates
        {
            get
            {
                return _numberOfAndGates;
            }
        }

        /// <summary>
        /// Gets the number of XOR gates in the circuit.
        /// </summary>
        public int NumberOfXorGates
        {
            get
            {
                return _numberOfXorGates;
            }
        }

        /// <summary>
        /// Gets the number of NOT gates in the circuit.
        /// </summary>
        public int NumberOfNotGates
        {
            get
            {
                return _numberOfNotGates;
            }
        }

        /// <summary>
        /// Gets the total number of gates in the circuit, including
        /// AND, XOR and NOT gates.
        /// </summary>
        public int NumberOfGates
        {
            get
            {
                return _numberOfAndGates + _numberOfXorGates + _numberOfNotGates;
            }
        }
    }
}
