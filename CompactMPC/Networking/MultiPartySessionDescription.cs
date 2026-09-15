using System.Collections.Generic;
using System.Linq;
using CompactMPC.Protocol;

namespace CompactMPC.Networking;

public class MultiPartySessionDescription(IEnumerable<Role> partyRoles)
{
    public MultiPartySessionDescription(params Role[] partyRoles) :
        this((IEnumerable<Role>)partyRoles) { }

    public MultiPartySessionDescription(int numberOfParties) :
        this(Enumerable.Repeat(Role.Default, numberOfParties)) { }

    public IReadOnlyList<PartySlot> PartySlots { get; } =
        partyRoles.Select(role => new PartySlot(role)).ToList();
    
    public IEnumerable<PartySlot> MatchingPartySlots(RoleMatcher roleMatcher) =>
        PartySlots.Where(partySlot => roleMatcher(partySlot.Role));
    
    public int NumberOfParties => PartySlots.Count;
}
