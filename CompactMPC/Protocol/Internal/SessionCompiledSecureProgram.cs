using System.Collections.Generic;
using CompactMPC.Collections;
using CompactMPC.Networking;

namespace CompactMPC.Protocol.Internal;

public class SessionCompiledSecureProgram(CompiledSecureProgram compiledProgram, OrderedMultiPartyNetworkSession session)
{
    private readonly IReadOnlyDictionary<Party, PartySlot> _partySlotsByParty = session.OrderedParties.Match(
        compiledProgram.SessionDescription.PartySlots,
        (party, partySlot) => partySlot.Accepts(party)
    );

    public CompiledPartyContext GetContext(Party party) => compiledProgram.GetContext(_partySlotsByParty[party]);
}
