using System.Collections.Generic;
using System.Linq;
using CompactMPC.Protocol;

namespace CompactMPC.Networking;

public record MultiPartySessionDescription(int NumberOfParties)
{
    public IReadOnlyList<PartySlot> PartySlots { get; } =
        Enumerable.Range(0, NumberOfParties).Select(_ => new PartySlot()).ToList();
}
