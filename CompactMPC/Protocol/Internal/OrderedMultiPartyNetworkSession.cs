using System.Collections.Generic;
using System.Linq;
using CompactMPC.Networking;

namespace CompactMPC.Protocol.Internal;

public class OrderedMultiPartyNetworkSession(IMultiPartyNetworkSession session)
{
    public IEnumerable<ITwoPartyNetworkSession> RemotePartySessions => session.RemotePartySessions;
    public IReadOnlyList<Party> OrderedParties { get; } = session.Parties.OrderBy(party => party.Guid).ToList();
    public bool IsLocalPartyLeading => OrderedParties[0] == LocalParty;
    public bool HasOddNumberOfParties => OrderedParties.Count % 2 != 0;
    public Party LocalParty => session.LocalParty;
}
