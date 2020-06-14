namespace CompactMPC.Circuits.Batching.Internal
{
    public interface ICircuitVisitor
    {
        void VisitAndGate();
        void VisitXorGate();
        void VisitNotGate();
    }
}
