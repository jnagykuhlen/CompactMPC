namespace CompactMPC.Circuits.Batching
{
    public readonly struct GateCount
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
        public int NumberOfInputGates { get; }

        /// <summary>
        /// Gets the number of output gates in the circuit.
        /// </summary>
        public int NumberOfOutputGates { get; }
        
        /// <summary>
        /// Creates a new set of information on a boolean circuit.
        /// </summary>
        /// <param name="numberOfAndGates">Number of AND gates in the circuit.</param>
        /// <param name="numberOfXorGates">Number of XOR gates in the circuit.</param>
        /// <param name="numberOfNotGates">Number of NOT gates in the circuit.</param>
        /// <param name="numberOfInputGates">Number of input gates in the circuit.</param>
        /// /// <param name="numberOfOutputGates">Number of output gates in the circuit.</param>
        private GateCount(int numberOfAndGates, int numberOfXorGates, int numberOfNotGates, int numberOfInputGates, int numberOfOutputGates)
        {
            NumberOfAndGates = numberOfAndGates;
            NumberOfXorGates = numberOfXorGates;
            NumberOfNotGates = numberOfNotGates;
            NumberOfInputGates = numberOfInputGates;
            NumberOfOutputGates = numberOfOutputGates;
        }

        public GateCount WithAndGate()
        {
            return new GateCount(
                NumberOfAndGates + 1,
                NumberOfXorGates,
                NumberOfNotGates,
                NumberOfInputGates,
                NumberOfOutputGates
            );
        }
        
        public GateCount WithXorGate()
        {
            return new GateCount(
                NumberOfAndGates,
                NumberOfXorGates + 1,
                NumberOfNotGates,
                NumberOfInputGates,
                NumberOfOutputGates
            );
        }
        
        public GateCount WithNotGate()
        {
            return new GateCount(
                NumberOfAndGates,
                NumberOfXorGates,
                NumberOfNotGates + 1,
                NumberOfInputGates,
                NumberOfOutputGates
            );
        }
        
        public GateCount WithInputGate()
        {
            return new GateCount(
                NumberOfAndGates,
                NumberOfXorGates,
                NumberOfNotGates,
                NumberOfInputGates + 1,
                NumberOfOutputGates
            );
        }
        
        public GateCount WithOutputGate()
        {
            return new GateCount(
                NumberOfAndGates,
                NumberOfXorGates,
                NumberOfNotGates,
                NumberOfInputGates,
                NumberOfOutputGates + 1
            );
        }
    }
}
