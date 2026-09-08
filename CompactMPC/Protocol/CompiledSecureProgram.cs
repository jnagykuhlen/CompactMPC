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
        if (_circuitStatistics == null)
        {
            var visitor = new StatisticsBatchCircuitVisitor();
            new ForwardCircuitEvaluation(visitor).Execute(
                sessionDescription.PartySlots.SelectMany(partySlot => GetInputContext(partySlot).Wires)
            );

            _circuitStatistics = visitor.GetCircuitStatistics();
        }

        return _circuitStatistics;
    }
}
