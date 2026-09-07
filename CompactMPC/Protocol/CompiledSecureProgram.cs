using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol;

public class CompiledSecureProgram(MultiPartySessionDescription sessionDescription, IReadOnlyDictionary<PartySlot, PartyInputContext> inputContextsByPartySlot, PartyOutputContext partyOutputContext)
{
    private CircuitStatistics? _circuitStatistics;

    public PartyInputContext GetInputContext(PartySlot partySlot) => inputContextsByPartySlot[partySlot];
    public PartyOutputContext OutputContext => partyOutputContext;
    public MultiPartySessionDescription SessionDescription => sessionDescription;

    public CircuitStatistics GetStatistics()
    {
        _circuitStatistics ??= ForwardCircuitEvaluation.CreateStatistics(
            sessionDescription.PartySlots.SelectMany(partySlot => GetInputContext(partySlot).Wires).ToArray(),
            OutputContext.NonConstantWires.ToArray()
        );

        return _circuitStatistics;
    }
}
