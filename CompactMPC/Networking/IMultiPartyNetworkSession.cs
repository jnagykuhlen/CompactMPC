using System;
using System.Collections.Generic;

namespace CompactMPC.Networking
{
    public interface IMultiPartyNetworkSession : IDisposable
    {
        IEnumerable<ITwoPartyNetworkSession> RemotePartySessions { get; }
        Party LocalParty { get; }
        int NumberOfParties { get; }
    }
}
