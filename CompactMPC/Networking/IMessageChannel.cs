using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public interface IMessageChannel
    {
        Task<byte[]> ReadMessageAsync();
        Task WriteMessageAsync(byte[] message);
    }
}
