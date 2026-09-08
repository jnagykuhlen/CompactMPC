namespace CompactMPC.Circuits;

public interface IBatchCircuitVisitor
{
    void VisitAndGateBatch(int numberOfAndGates);
    void VisitXorGate();
    void VisitNotGate();
}
