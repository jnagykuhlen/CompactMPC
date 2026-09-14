namespace CompactMPC.Protocol;

public class CompiledPartyContext(CompiledPartyInputContext inputContext, CompiledPartyOutputContext outputContext)
{
    public CompiledPartyInputContext InputContext { get; } = inputContext;
    public CompiledPartyOutputContext OutputContext { get; } = outputContext;
}
