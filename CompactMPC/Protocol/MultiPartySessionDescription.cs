using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.Protocol;

public record MultiPartySessionDescription(int NumberOfParties)
{
    public IReadOnlyList<PartySlot> PartySlots { get; } =
        Enumerable.Range(0, NumberOfParties).Select(_ => new PartySlot()).ToList();
}
