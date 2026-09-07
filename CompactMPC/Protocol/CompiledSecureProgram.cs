using System.Collections.Generic;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol;

public class CompiledSecureProgram(MultiPartySessionDescription sessionDescription, IReadOnlyDictionary<PartySlot, PartyInputContext> inputContextsByPartySlot, PartyOutputContext partyOutputContext)
{
    public PartyInputContext GetInputContext(PartySlot partySlot) => inputContextsByPartySlot[partySlot];
    public PartyOutputContext OutputContext => partyOutputContext;
    public MultiPartySessionDescription SessionDescription => sessionDescription;
}
