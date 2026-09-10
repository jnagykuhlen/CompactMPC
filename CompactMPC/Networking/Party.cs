using System;

namespace CompactMPC.Networking;

public record Party(Guid Guid, string Name, Role Role) : IComparable<Party>
{
    public Party(Guid guid, string name) : this(guid, name, Role.Default) { }
    public Party(Guid guid) : this(guid, $"Party {guid}") { }
    public Party() : this(Guid.NewGuid()) { }

    public static bool operator >(Party first, Party second) => first.CompareTo(second) > 0;
    public static bool operator <(Party first, Party second) => second > first;

    public int CompareTo(Party? other) => Guid.CompareTo(other?.Guid ?? Guid.Empty);
    public override string ToString() => Name;
}
