using System.Collections.Generic;
using System.Linq;
using CompactMPC.Networking;

namespace CompactMPC.Protocol;

public class MultiPartySessionDescription(IEnumerable<Role> partyRoles)
{
    public MultiPartySessionDescription(params Role[] partyRoles) :
        this((IEnumerable<Role>)partyRoles) { }

    public MultiPartySessionDescription(int numberOfParties) :
        this(Enumerable.Repeat(Role.Default, numberOfParties)) { }

    public IReadOnlyList<PartySlot> PartySlots { get; } =
        partyRoles.Select(role => new PartySlot(role)).ToList();
    
    public int NumberOfParties => PartySlots.Count;
}
