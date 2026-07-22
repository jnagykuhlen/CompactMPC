namespace CompactMPC.ObliviousTransfer.Preprocessing;

public class PreprocessedSenderBatch(BitQuadrupleArray options)
{
    public BitQuadruple GetOptions(int instanceId) => options[instanceId];
    public int NumberOfInstances => options.Length;
}
