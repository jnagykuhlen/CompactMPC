using System;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public interface ITwoPartyConnectionListener : IDisposable
    {
        Task<TcpTwoPartyNetworkSession> AcceptAsync();
    }
}