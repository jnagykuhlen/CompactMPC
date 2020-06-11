namespace CompactMPC.Circuits.Batching.Internal
{
    public class CountingCircuitVisitor : ICircuitVisitor
    {
        public int NumberOfAndGates { get; set; } = 0;
        public int NumberOfXorGates { get; set; } = 0;
        public int NumberOfNotGates { get; set; } = 0;
        public int NumberOfInputGates { get; set; } = 0;
        public int NumberOfOutputGates { get; set; } = 0;


        public void VisitAndGate()
        {
            NumberOfAndGates++;
        }

        public void VisitXorGate()
        {
            NumberOfXorGates++;
        }

        public void VisitNotGate()
        {
            NumberOfNotGates++;
        }

        public void VisitInputGate()
        {
            NumberOfInputGates++;
        }

        public void VisitOutputGate()
        {
            NumberOfOutputGates++;
        }
    }
}
