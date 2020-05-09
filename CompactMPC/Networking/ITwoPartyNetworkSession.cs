using System;

namespace CompactMPC.Networking
{
    public interface ITwoPartyNetworkSession : IDisposable
    {
        IMessageChannel Channel { get; }
        Party LocalParty { get; }
        Party RemoteParty { get; }
    }
}
