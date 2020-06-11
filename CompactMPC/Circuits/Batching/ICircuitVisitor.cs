namespace CompactMPC.Circuits.Batching
{
    public interface ICircuitVisitor
    {
        void VisitAndGate();
        void VisitXorGate();
        void VisitNotGate();
        void VisitInputGate();
        void VisitOutputGate();
    }
}
