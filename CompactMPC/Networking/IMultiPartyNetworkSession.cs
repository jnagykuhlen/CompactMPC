using System;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.Networking;

public interface IMultiPartyNetworkSession : IDisposable
{
    IEnumerable<ITwoPartyNetworkSession> RemotePartySessions { get; }
    Party LocalParty { get; }
}
