using System.Threading.Tasks;
using CompactMPC.Buffers;

namespace CompactMPC.Networking
{
    public interface IMessageChannel
    {
        Task<Message> ReadMessageAsync();
        Task WriteMessageAsync(Message message);
    }
}
