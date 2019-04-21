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
        [Obsolete("Use GetChannel instead.", false)]
        Stream GetConnection(int remotePartyId);
        IMessageChannel GetChannel(int remotePartyId);
        Party LocalParty { get; }
        IEnumerable<Party> RemoteParties { get; }
        int NumberOfParties { get; }
    }
}
