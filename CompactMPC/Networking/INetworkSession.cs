using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public interface INetworkSession : IDisposable
    {
        Stream GetConnection(int remotePartyId);
        Party LocalParty { get; }
        IEnumerable<Party> RemoteParties { get; }
        int NumberOfParties { get; }
    }
}
