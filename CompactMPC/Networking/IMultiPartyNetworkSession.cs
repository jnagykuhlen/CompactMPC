using System;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.Networking
{
    public interface IMultiPartyNetworkSession : IDisposable
    {
        IEnumerable<ITwoPartyNetworkSession> RemotePartySessions { get; }
        Party LocalParty { get; }
        int NumberOfParties { get; }
        
        IEnumerable<Party> Parties => RemotePartySessions.Select(session => session.RemoteParty).Append(LocalParty);
    }
}
