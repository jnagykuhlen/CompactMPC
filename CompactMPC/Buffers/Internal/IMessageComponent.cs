namespace CompactMPC.Buffers.Internal
{
    public interface IMessageComponent
    {
        void WriteToBuffer(byte[] messageBuffer, ref int offset);
        int Length { get; }
    }
}
