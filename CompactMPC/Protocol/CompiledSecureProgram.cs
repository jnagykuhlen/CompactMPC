using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;

namespace CompactMPC.Protocol;

public class CompiledSecureProgram(MultiPartySessionDescription sessionDescription, IReadOnlyDictionary<PartySlot, CompiledPartyContext> partyContexts)
{
    private CircuitStatistics? _circuitStatistics;

    public MultiPartySessionDescription SessionDescription { get; } = sessionDescription;

    public CompiledPartyContext GetContext(PartySlot partySlot) => partyContexts[partySlot];

    public CircuitStatistics GetStatistics()
    {
        if (_circuitStatistics == null)
        {
            var visitor = new StatisticsBatchCircuitVisitor();
            new ForwardCircuitEvaluation(visitor).Execute(
                SessionDescription.PartySlots.SelectMany(partySlot => GetContext(partySlot).InputContext.Wires)
            );

            _circuitStatistics = visitor.GetCircuitStatistics();
        }

        return _circuitStatistics;
    }
}
