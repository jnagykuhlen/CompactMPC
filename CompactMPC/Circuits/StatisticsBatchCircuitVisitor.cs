namespace CompactMPC.Circuits;

public class StatisticsBatchCircuitVisitor : IBatchCircuitVisitor
{
    private int _numberOfAndGates;
    private int _numberOfXorGates;
    private int _numberOfNotGates;
    private int _multiplicativeDepth;
    
    public void VisitAndGateBatch(int numberOfAndGates)
    {
        _numberOfAndGates += numberOfAndGates;
        _multiplicativeDepth++;
    }
    public void VisitXorGate() => _numberOfXorGates++;
    public void VisitNotGate() => _numberOfNotGates++;
    
    public CircuitStatistics GetCircuitStatistics() =>
        new(_numberOfAndGates, _numberOfXorGates, _numberOfNotGates, _multiplicativeDepth);
}
