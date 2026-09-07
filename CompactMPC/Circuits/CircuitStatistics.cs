namespace CompactMPC.Circuits;

public record CircuitStatistics(int NumberOfAndGates, int NumberOfXorGates, int NumberOfNotGates, int MultiplicativeDepth)
{
    public int TotalNumberOfGates => NumberOfAndGates + NumberOfXorGates + NumberOfNotGates;
}
