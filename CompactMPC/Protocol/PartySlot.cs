using CompactMPC.Networking;

namespace CompactMPC.Protocol;

public class PartySlot(Role role)
{
    public Role Role { get; } = role;
    public bool Accepts(Party party) => party.Role == Role;
}
