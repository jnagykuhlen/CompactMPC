namespace CompactMPC.Circuits
{
    /// <summary>
    /// Combines information on the number of gates in a boolean circuit.
    /// </summary>
    public class CircuitContext
    {
        /// <summary>
        /// Gets the number of AND gates in the circuit.
        /// </summary>
        public int NumberOfAndGates { get; }

        /// <summary>
        /// Gets the number of XOR gates in the circuit.
        /// </summary>
        public int NumberOfXorGates { get; }

        /// <summary>
        /// Gets the number of NOT gates in the circuit.
        /// </summary>
        public int NumberOfNotGates { get; }

        /// <summary>
        /// Gets the number of input gates in the circuit.
        /// </summary>
        public int NumberOfInputWires { get; }

        /// <summary>
        /// Gets the number of output gates in the circuit.
        /// </summary>
        public int NumberOfOutputWires { get; }
        
        /// <summary>
        /// Creates a new set of information on a boolean circuit.
        /// </summary>
        /// <param name="numberOfAndGates">Number of AND gates in the circuit.</param>
        /// <param name="numberOfXorGates">Number of XOR gates in the circuit.</param>
        /// <param name="numberOfNotGates">Number of NOT gates in the circuit.</param>
        /// <param name="numberOfInputWires">Number of input wires in the circuit.</param>
        /// /// <param name="numberOfOutputWires">Number of output wires in the circuit.</param>
        public CircuitContext(int numberOfAndGates, int numberOfXorGates, int numberOfNotGates, int numberOfInputWires, int numberOfOutputWires)
        {
            NumberOfAndGates = numberOfAndGates;
            NumberOfXorGates = numberOfXorGates;
            NumberOfNotGates = numberOfNotGates;
            NumberOfInputWires = numberOfInputWires;
            NumberOfOutputWires = numberOfOutputWires;
        }

        /// <summary>
        /// Gets the total number of gates in the circuit, including
        /// AND, XOR and NOT gates as well as input and output gates.
        /// </summary>
        // TODO: Exclude wires from calculation
        public int NumberOfGates
        {
            get
            {
                return NumberOfAndGates + NumberOfXorGates + NumberOfNotGates + NumberOfInputWires + NumberOfOutputWires;
            }
        }
    }
}
