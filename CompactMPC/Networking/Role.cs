namespace CompactMPC.Networking;

public record Role(string Name)
{
    public static readonly Role Default = new(nameof(Default));
}
