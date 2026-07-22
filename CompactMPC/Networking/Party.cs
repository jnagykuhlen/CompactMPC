using System;

namespace CompactMPC.Networking;

public record Party(int Id, string Name, Guid Guid)
{
    public Party(int id) : this(id, $"Party {id + 1}", Guid.NewGuid())
    { }
        
    public static bool operator>(Party first, Party second) => first.Guid.CompareTo(second.Guid) > 0;
    public static bool operator<(Party first, Party second) => second > first;
}
