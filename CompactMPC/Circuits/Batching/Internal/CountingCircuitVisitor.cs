namespace CompactMPC.Circuits.Batching.Internal
{
    public class CountingCircuitVisitor : ICircuitVisitor
    {
        public int NumberOfAndGates { get; private set; }
        public int NumberOfXorGates { get; private set; }
        public int NumberOfNotGates { get; private set; }

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
    }
}
