using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public interface IMultiPartyNetworkSession : IDisposable
    {
        IEnumerable<ITwoPartyNetworkSession> RemotePartySessions { get; }
        Party LocalParty { get; }
        int NumberOfParties { get; }
    }
}
