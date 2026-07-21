namespace CompactMPC.Circuits.Internal;

public interface ICircuitVisitor
{
    void VisitAndGate();
    void VisitXorGate();
    void VisitNotGate();
}
